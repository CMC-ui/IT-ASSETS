using System.ComponentModel.DataAnnotations;

namespace ItAssets.Models;

public class Asset
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AssetTag { get; set; } = string.Empty; // e.g., BLU-LAP-001

    [Required]
    [MaxLength(50)]
    public string DeviceType { get; set; } = string.Empty; // Laptops, Servers, NVRs, etc.

    [MaxLength(100)]
    public string MakeModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Supplier { get; set; } = string.Empty;

    public DateTime? DateReceived { get; set; }

    [MaxLength(50)]
    public string MacAddress { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    public string Details { get; set; } = "{}"; // JSON field for variable specs

    public decimal PurchasePrice { get; set; }

    public DateTime? WarrantyExpiration { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, In Repair, Decommissioned

    public bool UserSignedHandover { get; set; } = false;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsDeleted { get; set; } = false;

    public string Notes { get; set; } = string.Empty;

    public ICollection<AssetDocument> Documents { get; set; } = new List<AssetDocument>();
}
