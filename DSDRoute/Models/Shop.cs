namespace DSDRoute.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
        public virtual ICollection<CreditBill> CreditBills { get; set; } = new List<CreditBill>();
    }
}
