using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToursApp.Models
{
    public class TypeOfTour
    {
        [Key, Column(Order = 0)]
        public int TourId { get; set; }

        [Key, Column(Order = 1)]
        [StringLength(50)]
        public string TypeName { get; set; }

        public virtual Tour Tour { get; set; }
        public virtual Type Type { get; set; }
    }
}