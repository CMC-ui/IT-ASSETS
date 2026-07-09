using ItAssets.Components;
using ItAssets.Data;
using ItAssets.Models;
using ItAssets.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<BulkImportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var otherBranch = dbContext.Branches.FirstOrDefault(b => b.Name == "OTHER");
    if (otherBranch == null)
    {
        dbContext.Branches.Add(new Branch { Name = "OTHER" });
    }

    var otherDept = dbContext.Departments.FirstOrDefault(d => d.Name == "OTHER");
    if (otherDept == null)
    {
        dbContext.Departments.Add(new Department { Name = "OTHER" });
    }
    dbContext.SaveChanges();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Technician", "Standard User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var rootUser = await userManager.FindByNameAsync("Trevor") ?? await userManager.FindByNameAsync("trevor@system.local");
    if (rootUser == null)
    {
        rootUser = new ApplicationUser
        {
            UserName = "trevor@system.local",
            Email = "trevor@system.local",
            Name = "Trevor",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(rootUser, "S3sf1g1!3");
        if (!result.Succeeded)
        {
            var errs = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"FAILED TO CREATE ROOT USER: {errs}");
        }
        else
        {
            await userManager.AddToRoleAsync(rootUser, "Admin");
        }
    }
    else if (rootUser.UserName != "trevor@system.local")
    {
        rootUser.UserName = "trevor@system.local";
        await userManager.UpdateAsync(rootUser);
    }
    
    if (!dbContext.Users.Any(u => u.Name != "Trevor"))
    {
        var rawData = @"
HEASF OFFICE (Branch)

BLU TEAM (Department)
ALICIA = Alicia@Blu.local = 282
ANELYA = Anelya@Blu.local = 253
ANTOINETTE = Antoinette@Blu.local = 295
ANGELIQUE = Angelique@Blu.local = 225
DANIELLE = Danielle@Blu.local = 249
ELSIE = Elsie@Blu.local = 284
ERICKA = Ericka@Blu.local = 110
ISABELLA = Isabella@Blu.local = 288
KATHLEEN = Kathleen@Blu.local = 216
LEANDRE = Leandre@Blu.local = 266
LEE-ANNE = Lee-Anne@Blu.local = 270
LETHABO = Lethabo@Blu.local = 206
LUZILE = Luzile@Blu.local = 258
PHINDILE = Phindile@Blu.local = 254
RICHEL = Richel@Blu.local = 280
SHARNE = Sharne@Blu.local = 227
SIZAKELE = Sizakele@Blu.local = 111
WILNA = Wilna@Blu.local = 205
KAREN = Karen@Blu.local = 286
METRISHA = Metrisha@Blu.local = 202
MADELEINE = Madeleine@Blu.local = none
ADRIAAN = Adriaan@Blu.local = 294
ZENDA = Zenda@Blu.local = 207
BEVERLY = Beverly@Blu.local = 220

PAYROLL TEAM (Department)
MORNE = Morne@Payroll.local = 278
LIZ-MARI = Liz-Mari@Payroll.local = 226
NICOLE = Nicole@Payroll.local = 211
ELLA = Ella@Payroll.local = 221
PATIENCE = Patience@Payroll.local = 209
BERNICE = Bernice@Payroll.local = 230
CAREL = Carel@Payroll.local = 293

FISH TANK (Department)
GOMOLEMO = Gomolemo@FishTank.local = 259
LWANDLE = Lwandle@FishTank.local = 203
KABELO = Kabelo@FishTank.local = 275
THABANI = Thabani@FishTank.local = none
LESEGO = Lesego@FishTank.local = none

TRANSFERS (Department)
BERNICEW = BerniceW@Transfers.local = 213
OWEN = Owen@Transfers.local = 252
KATHLEENR = KathleenR@Transfers.local = 223
DEIRDRE = Deirdre@Transfers.local = 242

INVOICING (Department)
DERICK = Derick@Invoicing.local = 267
NIEL = Niel@Invoicing.local = 222
MICHELLE = Michelle@Invoicing.local = 228
PRINCESS = Princess@Invoicing.local = 210
JENET = Jenet@Invoicing.local = 200
LILLIAN = Lillian@Invoicing.local = 229
KYLE = Kyle@Invoicing.local = 217
SHARON = Sharon@Invoicing.local = 256

RECRUITMENT (Department)
HAPPINESSMA = HappinessMa@Recruit.local = none
NOMTHANDAZO = Nomthandazo@Recruit.local = 117
NONTSIKELELO = Nontsikelelo@Recruit.local = 283
SINAMISO = Sinamiso@Recruit.local = 264
PALESA = Palesa@Recruit.local = 218
ELZAAN = Elzaan@Recruit.local = 285
KHANYI = Khanyi@Recruit.local = 233
PHINDILER = PhindileR@Recruit.local = 232
HAPPINESSMO = HappinessMo@Recruit.local = none
JERRY = Jerry@Recruit.local = 246

DURBAN (Branch)
TOBIAS = Tobias@Durban.local = none
TUMELO = Tumelo@Durban.local = none
NKOSINATHI = Nkosinathi@Durban.local = none
NJABULO = Njabulo@Durban.local = 400
NOZIPHOP = NoziphoP@Durban.local = 401
YONELA = Yonela@Durban.local = 403
QHAWEKAZI = Qhawekazi@Durban.local = 402
ROSE = Rose@Durban.local = none
THOKOZANI = Thokozani@Durban.local = none
WESLEY = Wellington@Durban.local = 290
BONGANI = Bongani@Durban.local = none

GEORGE (Branch)
CAYRON = Cayron@George.local = none
KYLEG = Kyle@George.local = none
AYANDA = Ayanda@George.local = none
NARISSA = Narissa@George.local = 850

BLOEM (Branch)
DANIEL = Daniel@Bloem.local = none
JOHNNY = JohnnyY@Bloem.local = none
RASHIDA = Rashida@Bloem.local = 500

CAPE TOWN (Branch)
ALMO = Almo@Cape.local = none
MANDRY = Mandry@Cape.local = none
SAM = Sam@Cape.local = none
JAMES = James@Cape.local = none
JACQUES = Jacques@Cape.local = none
ANDRE = Andre@Cape.local = none
LOYISO = Loyiso@Cape.local = none
RENIER = Renier@Cape.local = none
KIM-LEE = Kim-Lee@Cape.local = 301
DENESIA = Denesia@Cape.local = 300
TANYA = Tanya@Cape.local = 304
RANDALL = Randall@Cape.local = 304
WENDY = Wendy@Cape.local = none

PORT ELIZABETH (Branch)
NIKELO = Nikelo@Port.local = none
TERRY = Terry@Port.local = none
NATASHA = Natasha@Port.local = none
DOMINIQUE = Dominique@Port.local = none
SINDISWA = Sindiswa@Port.local = none
";

        Branch? currentBranch = null;
        Department? currentDepartment = null;

        foreach (var line in rawData.Split('\n'))
        {
            var t = line.Trim();
            if (string.IsNullOrEmpty(t)) continue;

            if (t.Contains("(Branch)", StringComparison.OrdinalIgnoreCase))
            {
                var branchName = t.Replace("(Branch)", "").Trim();
                if (branchName.Equals("HEASF OFFICE", StringComparison.OrdinalIgnoreCase)) branchName = "HEAD OFFICE";

                currentBranch = dbContext.Branches.FirstOrDefault(b => b.Name == branchName);
                if (currentBranch == null)
                {
                    currentBranch = new Branch { Name = branchName };
                    dbContext.Branches.Add(currentBranch);
                    dbContext.SaveChanges();
                }
                currentDepartment = null;
            }
            else if (t.Contains("(Department)", StringComparison.OrdinalIgnoreCase))
            {
                var deptName = t.Replace("(Department)", "").Replace(".", "").Trim();
                currentDepartment = dbContext.Departments.FirstOrDefault(d => d.Name == deptName);
                if (currentDepartment == null)
                {
                    currentDepartment = new Department { Name = deptName };
                    dbContext.Departments.Add(currentDepartment);
                    dbContext.SaveChanges();
                }
            }
            else if (t.Contains("="))
            {
                var parts = t.Split('=').Select(p => p.Trim()).ToArray();
                if (parts.Length >= 2)
                {
                    var name = parts[0];
                    var email = parts[1];
                    var ext = parts.Length > 2 ? parts[2] : "none";
                    if (string.IsNullOrEmpty(ext)) ext = "none";

                    if (await userManager.FindByNameAsync(email) == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            Name = name,
                            Extension = ext,
                            BranchId = currentBranch?.Id,
                            DepartmentId = currentDepartment?.Id,
                            EmailConfirmed = true
                        };
                        var pwd = ext;
                        var result = await userManager.CreateAsync(newUser, pwd);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newUser, "Standard User");
                        }
                        else 
                        {
                            Console.WriteLine($"Failed to seed {email}: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
            }
        }
    }
}

// Identity routes (login/register) will need to be mapped if we use Identity UI or custom endpoints.
// For now we'll set up basic endpoints if needed, or rely on Blazor Identity pages.

app.Run();
