using System;
using System.Collections.Generic;

namespace ToursApp.Models;

public partial class Type
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
