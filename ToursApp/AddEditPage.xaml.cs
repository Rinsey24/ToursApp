using System.Windows;
using System.Windows.Controls;
using ToursApp.Models;
using ToursApp.Services;

namespace ToursApp
{
    public partial class AddEditPage : Page
    {
        private Hotel _currentHotel;

        public AddEditPage(Hotel hotel)
        {
            InitializeComponent();
            _currentHotel = hotel ?? new Hotel();

            DataContext = _currentHotel;
            CountryCombo.ItemsSource = ToursContext.GetInstance().Countries.ToList();

            if (_currentHotel.Id != 0)
            {
                NameBox.Text = _currentHotel.Name;
                StarsBox.Text = _currentHotel.CountOfStars.ToString();
                CountryCombo.SelectedItem = _currentHotel.CountryCodeNavigation;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                _currentHotel.Name = NameBox.Text;
                _currentHotel.CountOfStars = int.Parse(StarsBox.Text);
                _currentHotel.CountryCode = ((Country)CountryCombo.SelectedItem).Code;

                if (_currentHotel.Id == 0)
                {
                    HotelService.AddHotel(_currentHotel);
                }
                else
                {
                    HotelService.UpdateHotel(_currentHotel);
                }

                Manager.MainFrame.GoBack();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(StarsBox.Text) ||
                CountryCombo.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return false;
            }

            if (!int.TryParse(StarsBox.Text, out _))
            {
                MessageBox.Show("Количество звезд должно быть числом!");
                return false;
            }

            return true;
        }
    }
}