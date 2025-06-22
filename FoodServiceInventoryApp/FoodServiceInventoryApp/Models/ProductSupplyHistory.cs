using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodServiceInventoryApp.Models
{
    public class ProductSupplyHistory
    {
        [Key]
        public int SupplyRecordId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        [Required]
        public DateTime SupplyDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SuppliedQuantity { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal SupplyUnitPrice { get; set; }
    }
}