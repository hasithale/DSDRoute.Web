namespace DSDRoute.Models
{
    public enum ReturnStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Processed = 3
    }

    public class Return
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public string? ProcessedById { get; set; }
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
        public decimal? RefundAmount { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedById { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual Shop Shop { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual Order? Order { get; set; }
        public virtual ApplicationUser? ProcessedBy { get; set; }
        public virtual ApplicationUser? ApprovedBy { get; set; }
    }
}
