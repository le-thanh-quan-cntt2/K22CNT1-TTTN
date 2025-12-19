using System;
using System.Collections.Generic;

namespace MenFashionProject.Models;

public partial class ProductAttribute
{
    public int AttributeId { get; set; }

    public int? ProductId { get; set; }

    public string Size { get; set; } = null!;

    public string Color { get; set; } = null!;

    public int? Quantity { get; set; }

    public virtual Product? Product { get; set; }
}
