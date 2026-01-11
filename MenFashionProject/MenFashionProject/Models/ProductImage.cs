using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenFashionProject.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        public int? ProductId { get; set; }

        [StringLength(250)]
        public string? Url { get; set; }

        public bool? IsDefault { get; set; } = false;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}