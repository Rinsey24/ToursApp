using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToursApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ToursApp
{
    public partial class HotelPage : Page
    {
        private ToursContext _db = ToursContext.GetInstance();

        public HotelPage()
        {
            InitializeComponent();
            LoadHotels();
        }

        private void LoadHotels()
        {
            HotelsGrid.ItemsSource = _db.Hotels
                .Include(h => h.CountryCodeNavigation)
                .ToList();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу добавления
            Manager.MainFrame.Navigate(new AddEditPage(new Hotel()));
        }

        private void AddNewHotelDirectly()
        {
            try
            {
                var newHotel = new Hotel
                {
                    Id = 0, 
                    Name = "Новый отель",
                    CountOfStars = 4,
                    CountryCode = "JPN" 
                };

                _db.Hotels.Add(newHotel);
                _db.SaveChanges();

                LoadHotels(); 
                MessageBox.Show($"Отель добавлен! ID: {newHotel.Id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedHotel = (Hotel)((Button)sender).DataContext;
            Manager.MainFrame.Navigate(new AddEditPage(selectedHotel));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (HotelsGrid.SelectedItem is Hotel hotel)
            {
                _db.Hotels.Remove(hotel);
                _db.SaveChanges();
                LoadHotels();
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadHotels();
        }
    }
}