using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace ToursApp.Models;

public partial class HotelImage
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string ImageSource { get; set; } = null!;
    public virtual Hotel Hotel { get; set; } = null!;
    
    [NotMapped] 
    public BitmapImage ImageBitmap => new BitmapImage(new Uri(ImageSource));
}