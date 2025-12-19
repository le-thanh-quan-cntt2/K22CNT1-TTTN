using System;
using System.Collections.Generic;

namespace MenFashionProject.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public string Code { get; set; } = null!;

    public string? CustomerName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public decimal? TotalAmount { get; set; }

    public int? PaymentMethod { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
