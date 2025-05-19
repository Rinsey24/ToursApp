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
                _db.Hotels.Add(hotel);
                _db.SaveChanges();
                
                var imagePath = ImageService.GetImagePathForCountry(hotel.CountryCode);
                if (imagePath != null)
                {
                    _db.HotelImages.Add(new HotelImage
                    {
                        HotelId = hotel.Id,
                        ImageSource = imagePath
                    });
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении отеля: {ex.Message}");
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

        public static void AddHotelImage(int hotelId, string imagePath)
            {
                try
                {
                    var image = new HotelImage
                    {
                        HotelId = hotelId,
                        ImageSource = imagePath
                    };
        
                    _db.HotelImages.Add(image);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении изображения: {ex.Message}");
                }
            }
        }
    }
