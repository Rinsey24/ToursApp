using System;
using System.Collections.Generic;

namespace ToursApp.Models;

public partial class HotelImage
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public string ImageSource { get; set; } = null!;

    public virtual Hotel Hotel { get; set; } = null!;
}
