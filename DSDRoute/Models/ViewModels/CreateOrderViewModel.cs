using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DSDRoute.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        // Order Metadata
        [Display(Name = "Order No / Invoice No")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Time")]
        [DataType(DataType.Time)]
        public TimeSpan OrderTime { get; set; } = DateTime.Now.TimeOfDay;

        [Display(Name = "Sales Rep")]
        public string SalesRepId { get; set; } = string.Empty;

        [Display(Name = "Sales Rep Name")]
        public string SalesRepName { get; set; } = string.Empty;

        // Customer (Shop) Details
        [Required]
        [Display(Name = "Shop")]
        public int ShopId { get; set; }

        [Display(Name = "Shop Name")]
        public string ShopName { get; set; } = string.Empty;

        [Display(Name = "Shop Address")]
        public string ShopAddress { get; set; } = string.Empty;

        [Display(Name = "Contact No")]
        public string? ContactNo { get; set; }

        [Display(Name = "Previous Credit Bills Amount")]
        public decimal PreviousCreditAmount { get; set; } = 0;

        // Product List (Order Items)
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

        // Returned Items
        public List<ReturnItemViewModel> ReturnItems { get; set; } = new List<ReturnItemViewModel>();

        // Payment Details
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; } = 0;

        [Display(Name = "Invoice Discount")]
        public decimal InvoiceDiscount { get; set; } = 0;

        [Display(Name = "Total Returned")]
        public decimal TotalReturned { get; set; } = 0;

        [Display(Name = "Net Total")]
        public decimal NetTotal { get; set; } = 0;

        [Display(Name = "Today's Payment")]
        public decimal TodayPayment { get; set; } = 0;

        [Required]
        [Display(Name = "Payment Mode")]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        [Display(Name = "Outstanding Balance")]
        public decimal OutstandingBalance { get; set; } = 0;

        // Additional Info
        [Display(Name = "Notes / Remarks")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Tax / VAT %")]
        public decimal TaxPercentage { get; set; } = 0;

        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryDate { get; set; }

        // Dropdown lists
        public SelectList? Shops { get; set; }
        public SelectList? Products { get; set; }
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Unit Price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Discount per Item")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative")]
        public decimal DiscountPerItem { get; set; } = 0;

        [Display(Name = "Line Total")]
        public decimal LineTotal => (Quantity * UnitPrice) - (Quantity * DiscountPerItem);
    }

    public class ReturnItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Display(Name = "Reason for Return")]
        public ReturnReason ReturnReason { get; set; } = ReturnReason.Damaged;

        [Display(Name = "Custom Reason")]
        [StringLength(200)]
        public string? CustomReason { get; set; }

        [Required]
        [Display(Name = "Return Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Return Amount cannot be negative")]
        public decimal ReturnAmount { get; set; }
    }

    public enum PaymentMode
    {
        Cash = 0,
        Credit = 1,
        Card = 2,
        BankTransfer = 3
    }

    public enum ReturnReason
    {
        Expired = 0,
        Damaged = 1,
        Other = 2
    }
}