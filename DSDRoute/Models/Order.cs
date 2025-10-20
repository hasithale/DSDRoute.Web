namespace DSDRoute.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Approved = 1,
        InvoiceGenerated = 2,
        Delivered = 3,
        Cancelled = 4,
        Rejected = 5
    }

    public class Order
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string SalesRepId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public decimal TaxPercentage { get; set; } = 0;
        public decimal InvoiceDiscount { get; set; } = 0;
        public string? Notes { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveredById { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual Shop Shop { get; set; } = null!;
        public virtual ApplicationUser SalesRep { get; set; } = null!;
        public virtual ApplicationUser? DeliveredByUser { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}
