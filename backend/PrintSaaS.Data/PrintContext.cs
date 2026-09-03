using Microsoft.EntityFrameworkCore;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.Data;

public class PrintContext : DbContext
{
    public PrintContext(DbContextOptions<PrintContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobParameters> JobParameters => Set<JobParameters>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<QueueProfile> QueueProfiles => Set<QueueProfile>();
    public DbSet<TrayProfile> TrayProfiles => Set<TrayProfile>();
    public DbSet<MachineQueueName> MachineQueueNames => Set<MachineQueueName>();
    public DbSet<JobHistory> JobHistories => Set<JobHistory>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Job
        modelBuilder.Entity<Job>(e =>
        {
            e.HasKey(j => j.Id);
            e.Property(j => j.JobName).HasMaxLength(500).IsRequired();
            e.Property(j => j.ClientName).HasMaxLength(200).IsRequired();
            e.Property(j => j.FileLocation).HasMaxLength(1000).IsRequired();
            e.Property(j => j.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(j => j.JobType).HasConversion<string>().HasMaxLength(50);
            e.Property(j => j.PrinterJobId).HasMaxLength(200);
            e.HasOne(j => j.Printer).WithMany().HasForeignKey(j => j.PrinterId);
            e.HasOne(j => j.QueueProfile).WithMany().HasForeignKey(j => j.QueueProfileId);
            e.HasOne(j => j.Operator).WithMany().HasForeignKey(j => j.OperatorId);
            e.HasOne(j => j.Parameters).WithOne(p => p.Job).HasForeignKey<JobParameters>(p => p.JobId);
            e.HasMany(j => j.History).WithOne(h => h.Job).HasForeignKey(h => h.JobId);
            e.HasIndex(j => j.Status);
            e.HasIndex(j => j.CreatedAt);
        });

        // JobParameters
        modelBuilder.Entity<JobParameters>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.ColorMode).HasMaxLength(20);
            e.Property(p => p.PaperType).HasMaxLength(50);
        });

        // Printer
        modelBuilder.Entity<Printer>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Model).HasMaxLength(200);
            e.Property(p => p.IpAddress).HasMaxLength(50).IsRequired();
            e.Property(p => p.Protocol).HasMaxLength(10);
            e.Property(p => p.DefaultQueueName).HasMaxLength(200);
            e.Property(p => p.ColorCapability).HasMaxLength(20);
            e.Property(p => p.Status).HasMaxLength(100);
            e.HasMany(p => p.QueueProfiles).WithOne(q => q.Printer).HasForeignKey(q => q.PrinterId);
            e.HasMany(p => p.TrayProfiles).WithOne(t => t.Printer).HasForeignKey(t => t.PrinterId);
            e.HasMany(p => p.MachineQueueNames).WithOne(m => m.Printer).HasForeignKey(m => m.PrinterId);
        });

        // QueueProfile
        modelBuilder.Entity<QueueProfile>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(q => q.MachineQueueName).HasMaxLength(200).IsRequired();
            e.Property(q => q.ColorMode).HasMaxLength(20);
            e.HasOne(q => q.TrayProfile).WithMany().HasForeignKey(q => q.TrayProfileId);
        });

        // TrayProfile
        modelBuilder.Entity<TrayProfile>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.PaperType).HasMaxLength(50);
            e.Property(t => t.PaperSize).HasMaxLength(20);
            e.Property(t => t.PaperWeight).HasMaxLength(20);
        });

        // MachineQueueName
        modelBuilder.Entity<MachineQueueName>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.QueueName).HasMaxLength(200).IsRequired();
            e.Property(m => m.Description).HasMaxLength(500);
        });

        // JobHistory
        modelBuilder.Entity<JobHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Action).HasMaxLength(200).IsRequired();
            e.Property(h => h.Details).HasMaxLength(2000);
            e.HasOne(h => h.Operator).WithMany().HasForeignKey(h => h.OperatorId);
        });

        // AuditEntry — no cascade delete, compliance record
        modelBuilder.Entity<AuditEntry>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(200).IsRequired();
            e.Property(a => a.ComplianceNote).HasMaxLength(2000);
            e.HasOne(a => a.Job).WithMany().HasForeignKey(a => a.JobId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Operator).WithMany().HasForeignKey(a => a.OperatorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => a.Timestamp);
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.DisplayName).HasMaxLength(200);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(u => u.Username).IsUnique();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Printers
        modelBuilder.Entity<Printer>().HasData(
            new Printer
            {
                Id = 1, Name = "Nuvera 144MX", Model = "Xerox Nuvera 144MX",
                IpAddress = "192.168.1.101", Port = 443, Protocol = "IPPS",
                DefaultQueueName = "Nuvera", ColorCapability = "BW", IsActive = true
            },
            new Printer
            {
                Id = 2, Name = "Brenva HD", Model = "Xerox Brenva HD",
                IpAddress = "192.168.1.102", Port = 443, Protocol = "IPPS",
                DefaultQueueName = "Brenva", ColorCapability = "Color", IsActive = true
            },
            new Printer
            {
                Id = 3, Name = "Iridesse", Model = "Xerox Iridesse",
                IpAddress = "192.168.1.103", Port = 443, Protocol = "IPPS",
                DefaultQueueName = "Iridesse", ColorCapability = "Both", IsActive = true
            }
        );

        // Tray Profiles
        modelBuilder.Entity<TrayProfile>().HasData(
            new TrayProfile
            {
                Id = 1, PrinterId = 1, TrayNumber = 1,
                PaperType = "Plain", PaperSize = "Letter", PaperWeight = "75gsm", IsActive = true
            },
            new TrayProfile
            {
                Id = 2, PrinterId = 1, TrayNumber = 2,
                PaperType = "Bond", PaperSize = "Letter", PaperWeight = "90gsm", IsActive = true
            }
        );

        // Queue Profiles (MachineQueueName values are placeholders — admin must sync)
        modelBuilder.Entity<QueueProfile>().HasData(
            new QueueProfile
            {
                Id = 1, DisplayName = "PDF Duplex B&W", MachineQueueName = "Duplex-BW-Letter",
                PrinterId = 1, IsDuplex = true, ColorMode = "BW", Priority = 1,
                IsActive = true, TrayProfileId = 1
            },
            new QueueProfile
            {
                Id = 2, DisplayName = "PDF Simplex B&W", MachineQueueName = "Simplex-BW-Letter",
                PrinterId = 1, IsDuplex = false, ColorMode = "BW", Priority = 2,
                IsActive = true, TrayProfileId = 1
            },
            new QueueProfile
            {
                Id = 3, DisplayName = "Cheque Run", MachineQueueName = "Cheque-Duplex",
                PrinterId = 1, IsDuplex = true, ColorMode = "BW", Priority = 1,
                IsActive = true, TrayProfileId = 2
            },
            new QueueProfile
            {
                Id = 4, DisplayName = "Color Duplex", MachineQueueName = "Color-Duplex-Letter",
                PrinterId = 2, IsDuplex = true, ColorMode = "Color", Priority = 1,
                IsActive = true
            }
        );

        // Admin user — password: Admin@1234 (BCrypt hash)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1, Username = "admin", DisplayName = "Administrateur",
                PasswordHash = "$2a$11$placeholder_will_be_set_at_runtime",
                Role = UserRole.Admin, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
