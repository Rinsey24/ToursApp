using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media.Imaging;
using ToursApp.Services;

namespace ToursApp.Models
{
    public partial class Hotel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Range(1, 5)]
        public int CountOfStars { get; set; }

        [Required]
        [StringLength(3)]
        public string CountryCode { get; set; } = null!;

        [ForeignKey("CountryCode")]
        public virtual Country CountryCodeNavigation { get; set; } = null!;

        public virtual ICollection<HotelComment> HotelComments { get; set; } = new List<HotelComment>();
        public virtual ICollection<HotelImage> HotelImages { get; set; } = new List<HotelImage>();

        [NotMapped]
        public BitmapImage CountryImage => ImageService.GetCountryImage(CountryCode);

        [NotMapped]
        public string ImageTooltip => $"Отель {Name} в {CountryCodeNavigation?.Name} ({CountOfStars} звёзд)";
    }
}