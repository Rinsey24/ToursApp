using System;
using System.Collections.Generic;

namespace ToursApp.Models;

public partial class HotelComment
{
    public int Id { get; set; }

    public int HotelId { get; set; }

    public string Text { get; set; } = null!;

    public string Author { get; set; } = null!;

    public DateOnly CreationDate { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;
}
