using System.ComponentModel.DataAnnotations;

namespace ItAssets.Models;

public class AssetDocument
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DocumentType { get; set; } = "Document";

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
