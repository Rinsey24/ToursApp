using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ToursApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Manager.MainFrame = MainFrame;
            
            // Изначально открываем HotelPage
            MainFrame.Navigate(new HotelPage());
            BtnHotels.IsEnabled = false;
        }

        private void BtnTours_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainFrame.Content is ToursPage))
            {
                MainFrame.Navigate(new ToursPage());
                BtnTours.IsEnabled = false;
                BtnHotels.IsEnabled = true;
            }
        }

        private void BtnHotels_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainFrame.Content is HotelPage))
            {
                MainFrame.Navigate(new HotelPage());
                BtnHotels.IsEnabled = false;
                BtnTours.IsEnabled = true;
            }
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
                UpdateNavButtons();
            }
        }

        private void MainFrame_OnContentRendered(object sender, System.EventArgs e)
        {
            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;
            UpdateNavButtons();
        }

        private void UpdateNavButtons()
        {
            BtnHotels.IsEnabled = !(MainFrame.Content is HotelPage);
            BtnTours.IsEnabled = !(MainFrame.Content is ToursPage);
        }

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