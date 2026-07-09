using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ItAssets.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? Extension { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
