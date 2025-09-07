namespace TheLightStore.Models.Orders_Carts;

public partial class OrderInvoice
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public bool? InvoiceRequired { get; set; }

    public string? InvoiceType { get; set; }

    public string? IndividualName { get; set; }

    public string? CompanyName { get; set; }

    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    public string? InvoiceFileUrl { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
