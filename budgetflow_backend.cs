// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BudgetFlowPro.Data;
using BudgetFlowPro.Models;
using BudgetFlowPro.Services;
using BudgetFlowPro.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Application Services
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BudgetFlow Pro API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await context.Database.EnsureCreatedAsync();
    await SeedData.Initialize(context, userManager, roleManager);
}

app.Run();

// ========================================================================================
// Models/ApplicationUser.cs
namespace BudgetFlowPro.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<Budget> CreatedBudgets { get; set; } = new List<Budget>();
        public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
        public ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}

// ========================================================================================
// Models/Budget.cs
namespace BudgetFlowPro.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal SpentAmount { get; set; } = 0;
        public decimal RemainingAmount => TotalAmount - SpentAmount;
        public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
        public BudgetPeriod Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign Keys
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser CreatedBy { get; set; } = null!;
        
        // Navigation Properties
        public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
        public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }

    public enum BudgetStatus
    {
        Draft = 0,
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Active = 5,
        Completed = 6,
        Cancelled = 7
    }

    public enum BudgetPeriod
    {
        Monthly = 0,
        Quarterly = 1,
        Annual = 2
    }
}

// ========================================================================================
// Models/BudgetItem.cs
namespace BudgetFlowPro.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal SpentAmount { get; set; } = 0;
        public string CostCenter { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
    }
}

// ========================================================================================
// Models/Approval.cs
namespace BudgetFlowPro.Models
{
    public class Approval
    {
        public int Id { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public ApprovalLevel Level { get; set; }
        public string Comments { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        
        // Foreign Keys
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        
        public string? ReviewerId { get; set; }
        public ApplicationUser? Reviewer { get; set; }
    }

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        RequestMoreInfo = 3
    }

    public enum ApprovalLevel
    {
        Manager = 0,
        Finance = 1,
        Executive = 2
    }
}

// ========================================================================================
// Models/TimesheetEntry.cs
namespace BudgetFlowPro.Models
{
    public class TimesheetEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal TotalCost => Hours * HourlyRate;
        public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public string EmployeeId { get; set; } = string.Empty;
        public ApplicationUser Employee { get; set; } = null!;
        
        public int? BudgetId { get; set; }
        public Budget? Budget { get; set; }
    }

    public enum TimesheetStatus
    {
        Draft = 0,
        Submitted = 1,
        Approved = 2,
        Rejected = 3
    }
}

// ========================================================================================
// Models/AuditLog.cs
namespace BudgetFlowPro.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}

// ========================================================================================
// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BudgetFlowPro.Models;

namespace BudgetFlowPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetItem> BudgetItems { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Budget Configuration
            builder.Entity<Budget>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SpentAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.CreatedBy)
                      .WithMany(u => u.CreatedBudgets)
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BudgetItem Configuration
            builder.Entity<BudgetItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SpentAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Budget)
                      .WithMany(b => b.BudgetItems)
                      .HasForeignKey(e => e.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Approval Configuration
            builder.Entity<Approval>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Budget)
                      .WithMany(b => b.Approvals)
                      .HasForeignKey(e => e.BudgetId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Reviewer)
                      .WithMany(u => u.Approvals)
                      .HasForeignKey(e => e.ReviewerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TimesheetEntry Configuration
            builder.Entity<TimesheetEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hours).HasColumnType("decimal(4,2)");
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(8,2)");
                entity.HasOne(e => e.Employee)
                      .WithMany(u => u.TimesheetEntries)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Budget)
                      .WithMany()
                      .HasForeignKey(e => e.BudgetId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AuditLog Configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ApplicationUser Configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(8,2)");
            });
        }
    }
}

// ========================================================================================
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BudgetFlowPro.Models;
using BudgetFlowPro.DTOs;

namespace BudgetFlowPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, 
                            SignInManager<ApplicationUser> signInManager,
                            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    department = user.Department,
                    position = user.Position,
                    roles = roles
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Department = model.Department,
                Position = model.Position,
                HourlyRate = model.HourlyRate
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Employee");
            return Ok(new { message = "User created successfully" });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("Department", user.Department)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

// ========================================================================================
// Controllers/BudgetsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BudgetFlowPro.Services.Interfaces;
using BudgetFlowPro.DTOs;
using System.Security.Claims;

namespace BudgetFlowPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgets([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var budgets = await _budgetService.GetBudgetsAsync(userId, page, pageSize);
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(int id)
        {
            var budget = await _budgetService.GetBudgetByIdAsync(id);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var budget = await _budgetService.CreateBudgetAsync(dto, userId);
            return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, budget);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] UpdateBudgetDto dto)
        {
            var result = await _budgetService.UpdateBudgetAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var result = await _budgetService.DeleteBudgetAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitBudget(int id)
        {
            var result = await _budgetService.SubmitBudgetAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Budget submitted for approval" });
        }
    }
}

// ========================================================================================
// Services/Interfaces/IBudgetService.cs
using BudgetFlowPro.DTOs;
using BudgetFlowPro.Models;

namespace BudgetFlowPro.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetBudgetsAsync(string userId, int page, int pageSize);
        Task<BudgetDto?> GetBudgetByIdAsync(int id);
        Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto dto, string userId);
        Task<bool> UpdateBudgetAsync(int id, UpdateBudgetDto dto);
        Task<bool> DeleteBudgetAsync(int id);
        Task<bool> SubmitBudgetAsync(int id);
        Task<DashboardStatsDto> GetDashboardStatsAsync(string userId);
    }
}

// ========================================================================================
// Services/BudgetService.cs
using Microsoft.EntityFrameworkCore;
using BudgetFlowPro.Data;
using BudgetFlowPro.DTOs;
using BudgetFlowPro.Models;
using BudgetFlowPro.Services.Interfaces;

namespace BudgetFlowPro.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync(string userId, int page, int pageSize)
        {
            var budgets = await _context.Budgets
                .Include(b => b.CreatedBy)
                .Include(b => b.BudgetItems)
                .Where(b => b.CreatedById == userId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Department = b.Department,
                    TotalAmount = b.TotalAmount,
                    SpentAmount = b.SpentAmount,
                    RemainingAmount = b.RemainingAmount,
                    Status = b.Status.ToString(),
                    Period = b.Period.ToString(),
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    CreatedAt = b.CreatedAt,
                    CreatedBy = $"{b.CreatedBy.FirstName} {b.CreatedBy.LastName}",
                    ItemCount = b.BudgetItems.Count
                })
                .ToListAsync();

            return budgets;
        }

        public async Task<BudgetDto?> GetBudgetByIdAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.CreatedBy)
                .Include(b => b.BudgetItems)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null) return null;

            return new BudgetDto
            {
                Id = budget.Id,
                Title = budget.Title,
                Description = budget.Description,
                Department = budget.Department,
                TotalAmount = budget.TotalAmount,
                SpentAmount = budget.SpentAmount,
                RemainingAmount = budget.RemainingAmount,
                Status = budget.Status.ToString(),
                Period = budget.Period.ToString(),
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                CreatedAt = budget.CreatedAt,
                CreatedBy = $"{budget.CreatedBy.FirstName} {budget.CreatedBy.LastName}",
                ItemCount = budget.BudgetItems.Count,
                Items = budget.BudgetItems.Select(i => new BudgetItemDto
                {
                    Id = i.Id,
                    Category = i.Category,
                    Description = i.Description,
                    Amount = i.Amount,
                    SpentAmount = i.SpentAmount,
                    CostCenter = i.CostCenter
                }).ToList()
            };
        }

        public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto dto, string userId)
        {
            var budget = new Budget
            {
                Title = dto.Title,
                Description = dto.Description,
                Department = dto.Department,
                TotalAmount = dto.TotalAmount,
                Period = Enum.Parse<BudgetPeriod>(dto.Period),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedById = userId,
                Status = BudgetStatus.Draft
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            // Add budget items if provided
            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new BudgetItem
                    {
                        BudgetId = budget.Id,
                        Category = itemDto.Category,
                        Description = itemDto.Description,
                        Amount = itemDto.Amount,
                        CostCenter = itemDto.CostCenter
                    };
                    _context.BudgetItems.Add(item);
                }
                await _context.SaveChangesAsync();
            }

            return await GetBudgetByIdAsync(budget.Id);
        }

        public async Task<bool> UpdateBudgetAsync(int id, UpdateBudgetDto dto)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null || budget.Status != BudgetStatus.Draft) return false;

            budget.Title = dto.Title;
            budget.Description = dto.Description;
            budget.Department = dto.Department;
            budget.TotalAmount = dto.TotalAmount;
            budget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null || budget.Status != BudgetStatus.Draft) return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null || budget.Status != BudgetStatus.Draft) return false;

            budget.Status = BudgetStatus.Submitted;
            budget.UpdatedAt = DateTime.UtcNow;

            // Create approval workflow
            var approval = new Approval
            {
                BudgetId = id,
                Level = ApprovalLevel.Manager,
                Status = ApprovalStatus.Pending
            };
            _context.Approvals.Add(approval);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId)
        {
            var totalBudget = await _context.Budgets
                .Where(b => b.CreatedById == userId && b.Status == BudgetStatus.Active)
                .SumAsync(b => b.TotalAmount);

            var spentThisMonth = await _context.Budgets
                .Where(b => b.CreatedById == userId && b.Status == BudgetStatus.Active)
                .SumAsync(b => b.SpentAmount);

            var pendingApprovals = await _context.Approvals
                .CountAsync(a => a.Status == ApprovalStatus.Pending);

            var activeProjects = await _context.TimesheetEntries
                .Where(t => t.EmployeeId == userId)
                .Select(t => t.ProjectName)
                .Distinct()
                .CountAsync();

            return new DashboardStatsDto
            {
                TotalBudget = totalBudget,
                SpentThisMonth = spentThisMonth,
                PendingApprovals = pendingApprovals,
                ActiveProjects = activeProjects
            };
        }
    }
}

// ========================================================================================
// DTOs/BudgetDto.cs
namespace BudgetFlowPro.DTOs
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public List<BudgetItemDto> Items { get; set; } = new();
    }

    public class CreateBudgetDto
    {
        public string Title { get; set