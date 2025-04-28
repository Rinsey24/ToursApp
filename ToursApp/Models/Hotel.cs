using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ToursApp.Models;

public partial class Hotel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CountOfStars { get; set; }

    public string CountryCode { get; set; } = null!;

    public virtual Country CountryCodeNavigation { get; set; } = null!;

    public virtual ICollection<HotelComment> HotelComments { get; set; } = new List<HotelComment>();

    public virtual ICollection<HotelImage> HotelImages { get; set; } = new List<HotelImage>();
}
