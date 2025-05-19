using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ToursApp.Models;
using Type = ToursApp.Models.Type;

namespace ToursApp
{
    public partial class EditTourWindow : Window
    {
        private string _selectedImagePath;
        private string _originalImagePath;
        private const string DefaultImagePath = "pack://application:,,,/Resources/default_hotel.png";
        private const string ImageDirectory = @"C:\Users\Daria\Downloads\Country\ToursPictures";

        public string TourName { get; private set; }
        public string TourDescription { get; private set; }
        public decimal Price { get; private set; }
        public int TicketCount { get; private set; }
        public bool IsActual { get; private set; }
        public string SelectedImagePath { get; private set; }
        public string SelectedTypeName { get; private set; }

        public EditTourWindow(Tour tour)
        {
            InitializeComponent();
            
            // Гарантируем существование директории
            SafeCreateDirectory(ImageDirectory);

            // Инициализация UI
            TourNameTextBox.Text = tour.Name;
            TourDescriptionTextBox.Text = tour.Description;
            TourPriceTextBox.Text = tour.Price.ToString();
            TourTicketCountTextBox.Text = tour.TicketCount.ToString();
            IsActualCheckBox.IsChecked = tour.IsActual;
            TourFinishedCheckBox.IsChecked = !tour.IsActual && tour.TicketCount == 0;

            LoadTourTypes();

            // Загрузка изображения с защитой от ошибок
            LoadTourImage(tour);
        }

        private void SafeCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось создать директорию: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadTourImage(Tour tour)
        {
            if (string.IsNullOrEmpty(tour.ImagePreview))
            {
                ClearImageSelection();
                return;
            }

            try
            {
                string fullImagePath = Path.Combine(ImageDirectory, tour.ImagePreview);
                if (File.Exists(fullImagePath))
                {
                    _originalImagePath = tour.ImagePreview;
                    _selectedImagePath = fullImagePath;
                    ImagePreview.Source = SafeLoadImage(fullImagePath);
                }
                else
                {
                    ClearImageSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearImageSelection();
            }
        }

        private BitmapImage SafeLoadImage(string path)
        {
            const int maxAttempts = 3;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze();
                    return bitmap;
                }
                catch (IOException)
                {
                    if (i == maxAttempts - 1) throw;
                    Thread.Sleep(300);
                }
            }
            return new BitmapImage(new Uri(DefaultImagePath));
        }

        private void LoadTourTypes()
        {
            try
            {
                var db = ToursContext.GetInstance();
                var types = db.Types.ToList();
                TourTypeComboBox.ItemsSource = types;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов туров: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение для тура",
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _selectedImagePath = openFileDialog.FileName;
                    ImagePreview.Source = SafeLoadImage(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ClearImageSelection();
                }
            }
        }

        private void BtnClearImage_Click(object sender, RoutedEventArgs e)
        {
            ClearImageSelection();
        }

        private void ClearImageSelection()
        {
            _selectedImagePath = null;
            ImagePreview.Source = new BitmapImage(new Uri(DefaultImagePath));
        }

        private void TourFinished_Checked(object sender, RoutedEventArgs e)
        {
            TourTicketCountTextBox.Text = "0";
            TourTicketCountTextBox.IsEnabled = false;
            IsActualCheckBox.IsChecked = false;
            IsActualCheckBox.IsEnabled = false;
        }

        private void TourFinished_Unchecked(object sender, RoutedEventArgs e)
        {
            TourTicketCountTextBox.IsEnabled = true;
            IsActualCheckBox.IsEnabled = true;
        }

        private void BtnSaveTour_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            // Обработка изображения с повторными попытками
            if (!ProcessImage()) return;

            // Сохранение данных
            SaveTourData();
            
            DialogResult = true;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TourNameTextBox.Text))
            {
                ShowValidationError("Введите название тура!", TourNameTextBox);
                return false;
            }

            if (!decimal.TryParse(TourPriceTextBox.Text, out decimal price) || price <= 0)
            {
                ShowValidationError("Введите корректную цену (положительное число)!", TourPriceTextBox);
                return false;
            }

            if (!int.TryParse(TourTicketCountTextBox.Text, out int tickets) || tickets < 0)
            {
                ShowValidationError("Введите корректное количество билетов (целое неотрицательное число)!", TourTicketCountTextBox);
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message, TextBox control)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }

        private bool ProcessImage()
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                // Keep original image if no new one selected
                SelectedImagePath = _originalImagePath;
                return true;
            }

            try
            {
                string imageFileName = Path.GetFileName(_selectedImagePath);
                string destPath = Path.Combine(ImageDirectory, imageFileName);

                if (!string.Equals(_selectedImagePath, destPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Create directory if not exists
                    Directory.CreateDirectory(ImageDirectory);
            
                    // Copy with retries
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            File.Copy(_selectedImagePath, destPath, overwrite: true);
                            break;
                        }
                        catch when (i < 2)
                        {
                            Thread.Sleep(300);
                        }
                    }
                }

                SelectedImagePath = imageFileName;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void SafeCopyFile(string source, string dest)
        {
            const int maxAttempts = 3;
            const int delayMs = 300;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    File.Copy(source, dest, overwrite: true);
                    return;
                }
                catch (IOException)
                {
                    if (i == maxAttempts - 1) throw;
                    Thread.Sleep(delayMs);
                }
            }
        }

        private void SaveTourData()
        {
            TourName = TourNameTextBox.Text.Trim();
            TourDescription = TourDescriptionTextBox.Text.Trim();
            Price = decimal.Parse(TourPriceTextBox.Text);
            TicketCount = int.Parse(TourTicketCountTextBox.Text);
            IsActual = IsActualCheckBox.IsChecked == true;

            if (TourTypeComboBox.SelectedItem is Type selectedType)
            {
                SelectedTypeName = selectedType.Name;
            }
        }
    }
}