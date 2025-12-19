using System;
using System.Collections.Generic;

namespace MenFashionProject.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Alias { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public string? Image { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
