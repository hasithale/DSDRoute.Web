namespace DSDRoute.Models
{
    public enum PaymentType
    {
        Cash = 0,
        Cheque = 1,
        Credit = 2,
        CreditSettlement = 3
    }

    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? ChequeNumber { get; set; }
        public string? Notes { get; set; }
        public string? RecordedById { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerificationDate { get; set; }
        public string? VerifiedById { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual ApplicationUser? RecordedBy { get; set; }
        public virtual ApplicationUser? VerifiedBy { get; set; }
    }
}
