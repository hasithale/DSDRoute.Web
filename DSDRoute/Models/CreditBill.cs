using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSDRoute.Models
{
    public class CreditBill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShopId { get; set; }

        [ForeignKey("ShopId")]
        public virtual Shop Shop { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string InvoiceId { get; set; } = string.Empty;

        [Required]
        public string SalesRepId { get; set; } = string.Empty;

        [ForeignKey("SalesRepId")]
        public virtual ApplicationUser SalesRep { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }

        [Required]
        public bool IsSettled { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? SettledDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}