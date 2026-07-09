using System.ComponentModel.DataAnnotations;

namespace ItAssets.Models;

public class MaintenanceLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public string? TechnicianId { get; set; }
    public ApplicationUser? Technician { get; set; }

    [Required]
    public DateTime DateReported { get; set; } = DateTime.UtcNow;

    public DateTime? DateResolved { get; set; }

    [Required]
    [MaxLength(500)]
    public string IssueDescription { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ResolutionDetails { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Open"; // Open, In Progress, Resolved
}
