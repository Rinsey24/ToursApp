using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace ToursApp.Services
{
    public static class ImageService
    {
        private static readonly Dictionary<string, string> CountryImageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"USA", @"C:\users\Daria\Downloads\Country\usa.jpg"},
            {"FRA", @"C:\users\Daria\Downloads\Country\alps.jpg"},
            {"ESP", @"C:\users\Daria\Downloads\Country\spain.jpg"},
            {"JPN", @"C:\users\Daria\Downloads\Country\japan.jpg"},
            {"ITA", @"C:\users\Daria\Downloads\Country\italy.jpg"}
        };

        private static readonly ConcurrentDictionary<string, BitmapImage> ImageCache = new ConcurrentDictionary<string, BitmapImage>();

        public static BitmapImage GetCountryImage(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return DefaultImage;

            return ImageCache.GetOrAdd(countryCode, code =>
            {
                if (CountryImageMap.TryGetValue(code, out var path) && File.Exists(path))
                {
                    return new BitmapImage(new Uri(path));
                }
                return DefaultImage;
            });
        }

        public static BitmapImage DefaultImage { get; } = new BitmapImage(
            new Uri("pack://application:,,,/Resources/default_hotel.jpg"));

        public static string GetImagePathForCountry(string countryCode)
        {
            return CountryImageMap.TryGetValue(countryCode, out var path) ? path : null;
        }
    }
}