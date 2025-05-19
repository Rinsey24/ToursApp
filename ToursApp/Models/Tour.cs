using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ToursApp.Models
{
    public class Tour
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название тура обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Количество билетов обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Должен быть хотя бы 1 билет")]
        public int TicketCount { get; set; }

        public bool IsActual { get; set; } = true;
        
        [NotMapped]
        public bool IsFinished
        {
            get => TicketCount == 0 && !IsActual;
            set
            {
                if (value)
                {
                    TicketCount = 0;
                    IsActual = false;
                }
            }
        }
        [StringLength(255)]
        public string? ImagePreview { get; set; }

        [NotMapped]
        public ImageSource DisplayImage => GetImageSource();

        [NotMapped]
        public string PriceFormatted => $"{Price:C}";

        [NotMapped]
        public string TicketsLeft => $"Осталось билетов: {TicketCount}";

        public virtual ICollection<ToursApp.Models.Type> Types { get; set; } = new List<ToursApp.Models.Type>();
        public virtual ICollection<TypeOfTour> TypeOfTours { get; set; } = new List<TypeOfTour>();
       
        private ImageSource GetImageSource()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ImagePreview))
                    return new BitmapImage(new Uri("pack://application:,,,/Resources/default_hotel.png"));

                string imagePath = Path.Combine(@"C:\Users\Daria\Downloads\Country\ToursPictures", ImagePreview);
        
                if (File.Exists(imagePath))
                {
                    return new BitmapImage(new Uri(imagePath));
                }
        
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default_hotel.png"));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default_hotel.png"));
            }
        }
        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                errorMessage = "Название тура обязательно";
                return false;
            }

            if (Price <= 0)
            {
                errorMessage = "Цена должна быть положительной";
                return false;
            }

            if (TicketCount <= 0)
            {
                errorMessage = "Должен быть хотя бы 1 билет";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}