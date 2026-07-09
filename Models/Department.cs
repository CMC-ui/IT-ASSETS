using System.ComponentModel.DataAnnotations;

namespace ItAssets.Models;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
