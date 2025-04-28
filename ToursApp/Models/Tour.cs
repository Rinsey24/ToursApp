using System;
using System.Collections.Generic;

namespace ToursApp.Models;

public partial class Tour
{
    public int Id { get; set; }

    public int TicketCount { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImagePreview { get; set; }

    public decimal Price { get; set; }

    public bool IsActual { get; set; }

    public virtual ICollection<Type> TypeNames { get; set; } = new List<Type>();
}
