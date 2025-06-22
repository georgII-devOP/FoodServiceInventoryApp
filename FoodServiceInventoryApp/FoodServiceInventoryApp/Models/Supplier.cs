using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FoodServiceInventoryApp.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public ICollection<ProductSupplyHistory> ProductSupplyHistories { get; set; } = new List<ProductSupplyHistory>();
    }
}