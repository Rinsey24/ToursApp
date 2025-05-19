using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ToursApp.Models
{
    public class Type
    {
        [Key]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
        public virtual ICollection<TypeOfTour> TypeOfTours { get; set; } = new List<TypeOfTour>();
    }
}