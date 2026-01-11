using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenFashionProject.Models
{
    [Table("ProductAttributes")]
    public class ProductAttribute
    {
        [Key]
        public int AttributeId { get; set; }

        // SQL: ProductId INT NULL => C#: int?
        public int? ProductId { get; set; }

        [Required]
        [StringLength(10)]
        public string Size { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Color { get; set; } = null!;

        // SQL: Quantity INT DEFAULT 0 => C#: int (vì logic code cần số lượng cụ thể)
        public int? Quantity { get; set; } = 0;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}