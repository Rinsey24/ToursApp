using System.Windows;
using ToursApp.Models;

namespace ToursApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Инициализация главного фрейма
            Manager.MainFrame = MainFrame;
            
            // Загрузка стартовой страницы
            Manager.MainFrame.Navigate(new HotelPage());
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (Manager.MainFrame.CanGoBack)
            {
                Manager.MainFrame.GoBack();
            }
        }

        private void MainFrame_OnContentRendered(object sender, System.EventArgs e)
        {
            BtnBack.Visibility = Manager.MainFrame.CanGoBack 
                ? Visibility.Visible 
                : Visibility.Hidden;
        }

        // Добавляем отсутствующий метод для обработки закрытия окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (MessageBox.Show("Закрыть приложение?", "Подтверждение", 
            MessageBoxButton.YesNo) == MessageBoxResult.No)
             {
                 e.Cancel = true; 
             }
        }
    }
}