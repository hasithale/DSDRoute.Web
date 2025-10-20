namespace DSDRoute.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalShops { get; set; }
        public int TotalProducts { get; set; }
        public int PendingOrders { get; set; }
        public int TotalSalesReps { get; set; }
        public decimal TodaysOrderValue { get; set; }
        public decimal OutstandingCredits { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}