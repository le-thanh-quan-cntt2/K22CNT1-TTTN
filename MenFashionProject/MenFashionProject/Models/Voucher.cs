public class Voucher
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = "";

    /// <summary>
    /// 1 = Tiền, 2 = %
    /// </summary>
    public int? DiscountType { get; set; }

    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }
    public bool? IsActive { get; set; }
}
