using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MenFashionProject.Models;

[Table("Products")]
public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(250)]
    public string ProductName { get; set; } = null!;

    [StringLength(250)]
    public string? Alias { get; set; }

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public string? Detail { get; set; }

    [StringLength(250)]
    public string? Image { get; set; }

    // SỬA: Trong SQL là NOT NULL nên ở đây phải là decimal (không có dấu ?)
    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    public decimal? PriceSale { get; set; }

    public int? Quantity { get; set; } = 0;

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    // SỬA: Trong SQL là BIT (có thể null) nên dùng bool?
    public bool? IsActive { get; set; } = true;

    public bool? IsHome { get; set; } = false;

    public bool? IsHot { get; set; } = false;

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    // Quan hệ với bảng Thuộc tính (Size/Màu)
    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();

    // Quan hệ với bảng Ảnh phụ
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}