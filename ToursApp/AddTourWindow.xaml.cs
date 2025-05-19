using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using ToursApp.Models;
using Type = ToursApp.Models.Type;

namespace ToursApp
{
    public partial class AddTourWindow : Window
    {
        private string _currentFullImagePath;
        private const string DefaultImagePath = "pack://application:,,,/Resources/default_hotel.png";
        private const string ImageDirectory = @"C:\Users\Daria\Downloads\Country\ToursPictures";

        public string TourName { get; private set; }
        public string TourDescription { get; private set; }
        public decimal Price { get; private set; }
        public int TicketCount { get; private set; }
        public bool IsActual { get; private set; }
        public string SelectedImagePath { get; private set; }
        public string SelectedTypeName { get; private set; }

        public AddTourWindow()
        {
            InitializeComponent();
            EnsureImageDirectoryExists();
            ClearImageSelection();
            LoadTourTypes();
        }

        private void EnsureImageDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(ImageDirectory))
                {
                    Directory.CreateDirectory(ImageDirectory);
                    Debug.WriteLine($"[AddTourWindow] Директория создана: {ImageDirectory}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: Не удалось создать или проверить директорию для изображений: {ImageDirectory}\nОшибка: {ex.Message}\n\nПриложение может работать некорректно.",
                                "Ошибка директории", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTourTypes()
        {
            try
            {
                var db = ToursContext.GetInstance();
                var types = db.Types.ToList();
                TourTypeComboBox.ItemsSource = types;

                // Убираем выбор начального элемента, если это не критично
                // Если нужно сохранить логику выбора, используйте Name
                if (DataContext is Tour tour && tour.TypeOfTours?.Any() == true)
                {
                    var firstType = tour.TypeOfTours.First().Type;
                    TourTypeComboBox.SelectedItem = types.FirstOrDefault(t => t.Name == firstType.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static class FileOperations
        {
            public static string GetProcessesLockingFile(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    return $"Файл '{Path.GetFileName(filePath)}' не существует по пути: {filePath}";
                }

                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    {
                        // Если удалось открыть файл, значит он не заблокирован
                        return $"Файл '{Path.GetFileName(filePath)}' не заблокирован другими процессами.";
                    }
                }
                catch (IOException)
                {
                    // Файл заблокирован, попробуем определить, какими процессами
                    var processes = new StringBuilder();
                    processes.AppendLine($"Файл '{Path.GetFileName(filePath)}' (путь: {filePath}) заблокирован следующими процессами:");

                    try
                    {
                        foreach (var process in Process.GetProcesses())
                        {
                            try
                            {
                                foreach (ProcessModule module in process.Modules)
                                {
                                    if (module.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                                    {
                                        processes.AppendLine($"- {process.ProcessName} (PID: {process.Id})");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Игнорируем процессы, к которым нет доступа
                                Debug.WriteLine($"Ошибка при проверке процесса {process.ProcessName}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        processes.AppendLine($"- Ошибка при получении списка процессов: {ex.Message}");
                    }

                    return processes.ToString();
                }
                catch (Exception ex)
                {
                    return $"Исключение при анализе блокировок для '{Path.GetFileName(filePath)}': {ex}";
                }
            }

            public static void CopyFileAggressive(string sourcePath, string destinationPath, bool overwrite)
            {
                int attempts = 0;
                const int maxAttempts = 7;
                const int delayMs = 1500;

                Debug.WriteLine($"[CopyFileAggressive] Начало копирования: '{sourcePath}' -> '{destinationPath}', Overwrite: {overwrite}");

                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"Исходный файл не найден: {sourcePath}");
                }

                while (true)
                {
                    try
                    {
                        attempts++;
                        Debug.WriteLine($"[CopyFileAggressive] Попытка {attempts}/{maxAttempts} через System.IO.File.Copy...");
                        File.Copy(sourcePath, destinationPath, overwrite);
                        Debug.WriteLine("[CopyFileAggressive] System.IO.File.Copy успешно завершен.");
                        return;
                    }
                    catch (IOException ex) when ((ex.HResult & 0xFFFF) == 32 || (ex.HResult & 0xFFFF) == 33)
                    {
                        Debug.WriteLine($"[CopyFileAggressive] System.IO.File.Copy Ошибка IO (HResult: {ex.HResult & 0xFFFF}): {ex.Message}");
                        if (attempts >= maxAttempts)
                        {
                            Debug.WriteLine("[CopyFileAggressive] Достигнуто макс. кол-во попыток с System.IO.File.Copy.");
                            string blockingSource = GetProcessesLockingFile(sourcePath);
                            string blockingDest = GetProcessesLockingFile(destinationPath);
                            throw new IOException($"Не удалось скопировать файл '{Path.GetFileName(sourcePath)}' в '{Path.GetFileName(destinationPath)}' после {maxAttempts} попыток.\n" +
                                                  $"Источник: '{sourcePath}'.\nНазначение: '{destinationPath}'.\n" +
                                                  $"Сообщение системы: {ex.Message}\n\n" +
                                                  $"Информация о блокировке ИСТОЧНИКА:\n{blockingSource}\n\n" +
                                                  $"Информация о блокировке НАЗНАЧЕНИЯ:\n{blockingDest}", ex);
                        }
                        Debug.WriteLine($"[CopyFileAggressive] Ожидание {delayMs}ms перед следующей попыткой System.IO.File.Copy...");
                        Thread.Sleep(delayMs);
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        Debug.WriteLine($"[CopyFileAggressive] UnauthorizedAccessException: {uaEx.Message} на попытке {attempts}.");
                        if (attempts >= maxAttempts)
                        {
                            throw new IOException($"Не удалось скопировать файл '{Path.GetFileName(sourcePath)}' из-за проблем с правами доступа после {maxAttempts} попыток.", uaEx);
                        }
                        try
                        {
                            Debug.WriteLine($"[CopyFileAggressive] Попытка установить FileAttributes.Normal для '{sourcePath}'");
                            File.SetAttributes(sourcePath, FileAttributes.Normal);
                        }
                        catch (Exception exSetAttr)
                        {
                            Debug.WriteLine($"[CopyFileAggressive] Не удалось установить атрибуты для '{sourcePath}': {exSetAttr.Message}");
                        }
                        Debug.WriteLine($"[CopyFileAggressive] Ожидание {delayMs}ms после UnauthorizedAccessException перед следующей попыткой...");
                        Thread.Sleep(delayMs);
                    }
                    catch (Exception generalEx)
                    {
                        Debug.WriteLine($"[CopyFileAggressive] Неожиданная ошибка при копировании: {generalEx}");
                        throw;
                    }
                }
            }
        }

        private BitmapImage LoadImageWithoutLocking(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.WriteLine($"[LoadImageWithoutLocking] Путь к файлу пуст. Загрузка изображения по умолчанию.");
                return new BitmapImage(new Uri(DefaultImagePath));
            }

            if (!File.Exists(filePath))
            {
                if (filePath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        return new BitmapImage(new Uri(filePath));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LoadImageWithoutLocking] Ошибка загрузки pack URI '{filePath}': {ex.Message}. Загрузка изображения по умолчанию.");
                        return new BitmapImage(new Uri(DefaultImagePath));
                    }
                }
                Debug.WriteLine($"[LoadImageWithoutLocking] Файл не найден: {filePath}. Загрузка изображения по умолчанию.");
                return new BitmapImage(new Uri(DefaultImagePath));
            }

            try
            {
                var bitmap = new BitmapImage();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                Debug.WriteLine($"[LoadImageWithoutLocking] Изображение успешно загружено: {filePath}");
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadImageWithoutLocking] Ошибка загрузки изображения '{filePath}': {ex.Message}. Загрузка изображения по умолчанию.");
                MessageBox.Show($"Не удалось загрузить изображение: {Path.GetFileName(filePath)}\nПричина: {ex.Message}\nБудет показано изображение по умолчанию.", "Ошибка загрузки изображения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new BitmapImage(new Uri(DefaultImagePath));
            }
        }

        private void ReleaseImagePreview()
        {
            Debug.WriteLine("[ReleaseImagePreview] Попытка освободить ImagePreview.Source.");
            if (ImagePreview.Source != null)
            {
                var currentBitmap = ImagePreview.Source as BitmapImage;
                if (currentBitmap != null && currentBitmap.UriSource != null &&
                    currentBitmap.UriSource.OriginalString == DefaultImagePath)
                {
                    // Не делаем ничего особенного для дефолтного изображения
                }
            }
            ImagePreview.Source = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(150);
            Debug.WriteLine("[ReleaseImagePreview] ImagePreview.Source установлен в null, GC отработал.");
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение для тура",
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(_currentFullImagePath) ||
                    (ImagePreview.Source != null && (ImagePreview.Source as BitmapImage)?.UriSource?.OriginalString != DefaultImagePath))
                {
                    ReleaseImagePreview();
                }

                _currentFullImagePath = openFileDialog.FileName;
                Debug.WriteLine($"[BtnSelectImage_Click] Выбрано изображение: {_currentFullImagePath}");
                ImagePreview.Source = LoadImageWithoutLocking(_currentFullImagePath);
            }
        }

        private void BtnClearImage_Click(object sender, RoutedEventArgs e)
        {
            ClearImageSelection();
        }

        private void ClearImageSelection()
        {
            ReleaseImagePreview();
            _currentFullImagePath = null;
            SelectedImagePath = null;
            ImagePreview.Source = LoadImageWithoutLocking(DefaultImagePath);
            Debug.WriteLine("[ClearImageSelection] Выбор изображения очищен, установлено изображение по умолчанию.");
        }

        private void TourFinished_Checked(object sender, RoutedEventArgs e)
        {
            txtTickets.Text = "0";
            txtTickets.IsEnabled = false;
            chkIsActual.IsChecked = false;
            chkIsActual.IsEnabled = false;
        }

        private void TourFinished_Unchecked(object sender, RoutedEventArgs e)
        {
            txtTickets.Text = "1";
            txtTickets.IsEnabled = true;
            chkIsActual.IsEnabled = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название тура!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (положительное число)!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            bool isFinished = chkTourFinished.IsChecked == true;
            int tickets;

            if (isFinished)
            {
                tickets = 0;
            }
            else
            {
                if (!int.TryParse(txtTickets.Text, out tickets) || tickets < 0)
                {
                    MessageBox.Show("Для активного тура укажите корректное количество билетов (целое неотрицательное число)!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTickets.Focus();
                    return;
                }
            }

            string finalImageNameOnly = null;

            if (!string.IsNullOrEmpty(_currentFullImagePath) && _currentFullImagePath != DefaultImagePath)
            {
                EnsureImageDirectoryExists();
                string sourceFileName = Path.GetFileName(_currentFullImagePath);
                string destinationPath = Path.Combine(ImageDirectory, sourceFileName);

                Debug.WriteLine($"[BtnSave_Click] Подготовка к сохранению изображения. Текущий полный путь: '{_currentFullImagePath}', Назначение: '{destinationPath}'");

                bool previewIsSource = false;
                bool previewIsDestination = false;

                if (ImagePreview.Source is BitmapImage bmp && bmp.UriSource != null && bmp.UriSource.IsAbsoluteUri && bmp.UriSource.IsFile)
                {
                    previewIsSource = string.Equals(bmp.UriSource.LocalPath, _currentFullImagePath, StringComparison.OrdinalIgnoreCase);
                    previewIsDestination = string.Equals(bmp.UriSource.LocalPath, destinationPath, StringComparison.OrdinalIgnoreCase);
                }

                if (previewIsSource || previewIsDestination || (File.Exists(destinationPath) && !string.Equals(_currentFullImagePath, destinationPath, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"[BtnSave_Click] ImagePreview связан с источником/назначением или файл назначения существует. Освобождаем ImagePreview...");
                    ReleaseImagePreview();
                    ImagePreview.Source = LoadImageWithoutLocking(DefaultImagePath);
                }

                try
                {
                    if (!string.Equals(Path.GetFullPath(_currentFullImagePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[BtnSave_Click] Вызов CopyFileAggressive. Источник: '{_currentFullImagePath}', Назначение: '{destinationPath}'");
                        FileOperations.CopyFileAggressive(_currentFullImagePath, destinationPath, true);
                        _currentFullImagePath = destinationPath;
                        Debug.WriteLine($"[BtnSave_Click] Копирование успешно. Новый _currentFullImagePath: '{_currentFullImagePath}'");
                    }
                    else
                    {
                         Debug.WriteLine($"[BtnSave_Click] Файл '{sourceFileName}' уже находится в '{ImageDirectory}' и является выбранным. Копирование не требуется.");
                         if (!File.Exists(_currentFullImagePath))
                         {
                            throw new FileNotFoundException($"Файл {_currentFullImagePath} должен был существовать в целевой директории, но не найден. Возможно, он был удален.");
                         }
                    }
                    finalImageNameOnly = sourceFileName;
                    ImagePreview.Source = LoadImageWithoutLocking(_currentFullImagePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BtnSave_Click] ИСКЛЮЧЕНИЕ при сохранении изображения: {ex}");
                    MessageBox.Show($"Ошибка при сохранении изображения:\n{ex.Message}\n\nДетальная информация (см. также окно Вывод):\n{(ex.InnerException != null ? ex.InnerException.Message : "Нет внутренней ошибки")}",
                                    "Ошибка сохранения изображения", MessageBoxButton.OK, MessageBoxImage.Error);
                    ClearImageSelection();
                    return;
                }
            }
            else
            {
                SelectedImagePath = null;
                if (_currentFullImagePath == DefaultImagePath)
                {
                     Debug.WriteLine("[BtnSave_Click] Использовано изображение по умолчанию. Копирование не требуется.");
                }
                else
                {
                     Debug.WriteLine("[BtnSave_Click] Изображение не было выбрано или было очищено.");
                }
            }

            // Handle tour type
            if (TourTypeComboBox.SelectedItem is Type selectedType)
            {
                SelectedTypeName = selectedType.Name;
            }

            TourName = txtName.Text.Trim();
            TourDescription = txtDescription.Text.Trim();
            Price = price;
            TicketCount = tickets;
            IsActual = isFinished ? false : chkIsActual.IsChecked == true;
            SelectedImagePath = finalImageNameOnly;

            DialogResult = true;
            Close();
        }

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFullImagePath) || _currentFullImagePath == DefaultImagePath)
            {
                MessageBox.Show("Сначала выберите изображение (не по умолчанию)!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnsureImageDirectoryExists();
            string imageFileNameOnly = Path.GetFileName(_currentFullImagePath);
            string destPath = Path.Combine(ImageDirectory, imageFileNameOnly);
            Debug.WriteLine($"[BtnSaveImage_Click] Попытка сохранить изображение: '{_currentFullImagePath}' -> '{destPath}'");

            try
            {
                bool previewNeedsRelease = false;
                if (ImagePreview.Source is BitmapImage bmp && bmp.UriSource != null && bmp.UriSource.IsAbsoluteUri && bmp.UriSource.IsFile)
                {
                    if (string.Equals(bmp.UriSource.LocalPath, _currentFullImagePath, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(bmp.UriSource.LocalPath, destPath, StringComparison.OrdinalIgnoreCase))
                    {
                        previewNeedsRelease = true;
                    }
                }

                if (previewNeedsRelease || (File.Exists(destPath) && !string.Equals(Path.GetFullPath(_currentFullImagePath), Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase)))
                {
                     Debug.WriteLine($"[BtnSaveImage_Click] Освобождаем ImagePreview перед сохранением изображения.");
                     ReleaseImagePreview();
                     ImagePreview.Source = LoadImageWithoutLocking(DefaultImagePath);
                }

                if (File.Exists(destPath) && !string.Equals(Path.GetFullPath(_currentFullImagePath), Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase))
                {
                    var result = MessageBox.Show($"Файл '{imageFileNameOnly}' уже существует в папке '{ImageDirectory}'. Перезаписать?",
                                                "Подтверждение перезаписи", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        if (!string.IsNullOrEmpty(_currentFullImagePath)) ImagePreview.Source = LoadImageWithoutLocking(_currentFullImagePath);
                        return;
                    }
                }

                if (!string.Equals(Path.GetFullPath(_currentFullImagePath), Path.GetFullPath(destPath), StringComparison.OrdinalIgnoreCase))
                {
                    FileOperations.CopyFileAggressive(_currentFullImagePath, destPath, true);
                    _currentFullImagePath = destPath;
                }

                ImagePreview.Source = LoadImageWithoutLocking(_currentFullImagePath);
                SelectedImagePath = imageFileNameOnly;

                MessageBox.Show("Изображение успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BtnSaveImage_Click] ИСКЛЮЧЕНИЕ при сохранении изображения: {ex}");
                MessageBox.Show($"Ошибка при сохранении изображения:\n{ex.Message}\n\nДетали (см. также окно Вывод):\n{(ex.InnerException != null ? ex.InnerException.Message : "Нет внутренней ошибки")}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                if(!string.IsNullOrEmpty(_currentFullImagePath) && File.Exists(_currentFullImagePath))
                    ImagePreview.Source = LoadImageWithoutLocking(_currentFullImagePath);
                else
                    ImagePreview.Source = LoadImageWithoutLocking(DefaultImagePath);
            }
        }
    }
}
