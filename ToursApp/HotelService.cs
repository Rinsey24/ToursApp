using ToursApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ToursApp.Services
{
    public static class HotelService
    {
        private static ToursContext _db = ToursContext.GetInstance();

        public static void AddHotel(Hotel hotel)
        {
            try
            {
                hotel.Id = 0; 
                _db.Hotels.Add(hotel);
                _db.SaveChanges();
                MessageBox.Show($"Отель добавлен! ID: {hotel.Id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public static void UpdateHotel(Hotel hotel)
        {
            try
            {
                _db.Entry(hotel).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public static void DeleteHotel(int id)
        {
            try
            {
                var hotel = _db.Hotels.Find(id);
                if (hotel != null)
                {
                    _db.Hotels.Remove(hotel);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}