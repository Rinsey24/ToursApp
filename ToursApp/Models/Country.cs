﻿using System;
using System.Collections.Generic;

namespace ToursApp.Models;

public partial class Country
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
