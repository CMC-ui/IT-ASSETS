using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ItAssets.Data;
using ItAssets.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace ItAssets.Services;

public class AssetCsvRecord
{
    public string AssetTag { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string MakeModel { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime? WarrantyExpiration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}

public class BulkImportService
{
    private readonly ApplicationDbContext _context;

    public BulkImportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> ImportAssetsAsync(Stream csvStream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<AssetCsvRecord>().ToList();
        int importedCount = 0;

        foreach (var record in records)
        {
            // Skip if asset already exists
            if (await _context.Assets.AnyAsync(a => a.AssetTag == record.AssetTag))
                continue;

            Branch? branch = null;
            if (!string.IsNullOrWhiteSpace(record.BranchName))
            {
                branch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == record.BranchName);
                if (branch == null)
                {
                    branch = new Branch { Name = record.BranchName };
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                }
            }

            Department? department = null;
            if (!string.IsNullOrWhiteSpace(record.DepartmentName))
            {
                department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == record.DepartmentName);
                if (department == null)
                {
                    department = new Department { Name = record.DepartmentName };
                    _context.Departments.Add(department);
                    await _context.SaveChangesAsync();
                }
            }

            var asset = new Asset
            {
                AssetTag = record.AssetTag,
                DeviceType = record.DeviceType,
                MakeModel = record.MakeModel,
                MacAddress = record.MacAddress,
                IpAddress = record.IpAddress,
                PurchasePrice = record.PurchasePrice,
                WarrantyExpiration = record.WarrantyExpiration,
                Status = string.IsNullOrWhiteSpace(record.Status) ? "Active" : record.Status,
                BranchId = branch?.Id,
                DepartmentId = department?.Id
            };

            _context.Assets.Add(asset);
            importedCount++;
        }

        await _context.SaveChangesAsync();
        return importedCount;
    }
}
