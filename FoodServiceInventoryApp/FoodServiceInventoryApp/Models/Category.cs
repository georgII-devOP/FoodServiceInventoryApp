using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FoodServiceInventoryApp.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
