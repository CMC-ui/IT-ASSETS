using System.ComponentModel.DataAnnotations;

namespace ItAssets.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    public string? UserId { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    public int? AssetId { get; set; }
}
