using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<ServiceProviderProfile> ServiceProviderProfiles => Set<ServiceProviderProfile>();

    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();

    public DbSet<ServiceProviderSkill> ServiceProviderSkills => Set<ServiceProviderSkill>();

    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    public DbSet<ServiceRequestStatusHistory> ServiceRequestStatusHistories => Set<ServiceRequestStatusHistory>();

    public DbSet<ServiceProviderApprovalHistory> ServiceProviderApprovalHistories => Set<ServiceProviderApprovalHistory>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ServiceProviderCV> ServiceProviderCVs => Set<ServiceProviderCV>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureCategoryAndSkill(builder);
        ConfigureServiceProviderProfile(builder);
        ConfigureClientProfile(builder);
        ConfigureServiceProviderSkill(builder);
        ConfigurePortfolioItem(builder);
        ConfigureReview(builder);
        ConfigureFavorite(builder);
        ConfigureReport(builder);
        ConfigureServiceRequest(builder);
        ConfigureServiceProviderApprovalHistory(builder);
        ConfigureNotification(builder);
        ConfigureServiceProviderCV(builder);
        ConfigureReport(builder);
        ConfigureServiceRequest(builder);
        ConfigureServiceProviderApprovalHistory(builder);
        ConfigureNotification(builder);
    }

    private static void ConfigureCategoryAndSkill(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        builder.Entity<Skill>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(s => s.Category)
                .WithMany(c => c.Skills)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // A category shouldn't list the same skill name twice.
            entity.HasIndex(s => new { s.CategoryId, s.Name }).IsUnique();
        });
    }

    private static void ConfigureServiceProviderProfile(ModelBuilder builder)
    {
        builder.Entity<ServiceProviderProfile>(entity =>
        {
            entity.Property(f => f.Headline).IsRequired().HasMaxLength(150);
            entity.Property(f => f.HourlyRate).HasColumnType("decimal(10,2)");
            entity.Property(f => f.AverageRating).HasColumnType("decimal(3,2)");

            // One profile per user account.
            entity.HasOne(f => f.User)
                .WithOne(u => u.ServiceProviderProfile)
                .HasForeignKey<ServiceProviderProfile>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Category)
                .WithMany(c => c.ServiceProviderProfiles)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Directory search filters by category and sorts by rating —
            // this composite index serves both in one pass.
            entity.HasIndex(f => new { f.CategoryId, f.AverageRating });
            entity.HasIndex(f => f.Address);

            // Every directory search filters on this — only Approved
            // profiles are ever returned to clients.
            entity.HasIndex(f => f.ApprovalStatus);
        });
    }

    private static void ConfigureClientProfile(ModelBuilder builder)
    {
        builder.Entity<ClientProfile>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<ClientProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureServiceProviderSkill(ModelBuilder builder)
    {
        builder.Entity<ServiceProviderSkill>(entity =>
        {
            entity.HasKey(fs => new { fs.ServiceProviderProfileId, fs.SkillId });

            entity.HasOne(fs => fs.ServiceProviderProfile)
                .WithMany(f => f.ServiceProviderSkills)
                .HasForeignKey(fs => fs.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(fs => fs.Skill)
                .WithMany(s => s.ServiceProviderSkills)
                .HasForeignKey(fs => fs.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePortfolioItem(ModelBuilder builder)
    {
        builder.Entity<PortfolioItem>(entity =>
        {
            entity.Property(p => p.Title).IsRequired().HasMaxLength(150);

            entity.HasOne(p => p.ServiceProviderProfile)
                .WithMany(f => f.PortfolioItems)
                .HasForeignKey(p => p.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.ServiceProviderProfileId, p.DisplayOrder });
        });
    }

    private static void ConfigureReview(ModelBuilder builder)
    {
        builder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment).IsRequired().HasMaxLength(1000);

            entity.HasOne(r => r.ServiceProviderProfile)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ClientProfile)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // A client may only review the same service provider once.
            entity.HasIndex(r => new { r.ServiceProviderProfileId, r.ClientProfileId }).IsUnique();
        });
    }

    private static void ConfigureFavorite(ModelBuilder builder)
    {
        builder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => new { f.ClientProfileId, f.ServiceProviderProfileId });

            entity.HasOne(f => f.ClientProfile)
                .WithMany(c => c.Favorites)
                .HasForeignKey(f => f.ClientProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.ServiceProviderProfile)
                .WithMany(f => f.FavoritedBy)
                .HasForeignKey(f => f.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReport(ModelBuilder builder)
    {
        builder.Entity<Report>(entity =>
        {
            entity.Property(r => r.Details).IsRequired().HasMaxLength(1000);

            // Reports reference three different user relationships on the
            // same ApplicationUser table, so every one but the reporter
            // must be Restrict — SQL Server rejects multiple cascade paths
            // that could reach the same row twice.
            entity.HasOne(r => r.ReporterUser)
                .WithMany()
                .HasForeignKey(r => r.ReporterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ResolvedByUser)
                .WithMany()
                .HasForeignKey(r => r.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReportedReview)
                .WithMany()
                .HasForeignKey(r => r.ReportedReviewId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.Status);
        });
    }

    private static void ConfigureServiceRequest(ModelBuilder builder)
    {
        builder.Entity<ServiceRequest>(entity =>
        {
            entity.Property(r => r.Title).IsRequired().HasMaxLength(150);
            entity.Property(r => r.Description).IsRequired().HasMaxLength(2000);

            // Mirrors Review's pattern: the service provider side cascades
            // (this request is entirely their business), the client side is
            // Restrict so a client account removal can't silently wipe a
            // provider's request history.
            entity.HasOne(r => r.ServiceProviderProfile)
                .WithMany(f => f.ServiceRequests)
                .HasForeignKey(r => r.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ClientProfile)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(r => r.ClientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Both dashboards list "my requests" filtered/sorted by status.
            entity.HasIndex(r => new { r.ServiceProviderProfileId, r.Status });
            entity.HasIndex(r => new { r.ClientProfileId, r.Status });
        });

        builder.Entity<ServiceRequestStatusHistory>(entity =>
        {
            entity.HasOne(h => h.ServiceRequest)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(h => h.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(h => new { h.ServiceRequestId, h.ChangedOn });
        });
    }

    private static void ConfigureServiceProviderApprovalHistory(ModelBuilder builder)
    {
        builder.Entity<ServiceProviderApprovalHistory>(entity =>
        {
            entity.HasOne(h => h.ServiceProviderProfile)
                .WithMany(f => f.ApprovalHistory)
                .HasForeignKey(h => h.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(h => new { h.ServiceProviderProfileId, h.ChangedOn });
        });
    }

    private static void ConfigureNotification(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Title).IsRequired().HasMaxLength(150);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
            entity.Property(n => n.LinkUrl).HasMaxLength(300);
            entity.Property(n => n.RelatedEntityType).HasMaxLength(50);

            entity.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedOn });
        });
    }

    private static void ConfigureServiceProviderCV(ModelBuilder builder)
    {
        builder.Entity<ServiceProviderCV>(entity =>
        {
            entity.Property(cv => cv.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(cv => cv.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(cv => cv.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(cv => cv.ContentType).IsRequired().HasMaxLength(100);

            entity.HasOne(cv => cv.ServiceProviderProfile)
                .WithOne(sp => sp.CV)
                .HasForeignKey<ServiceProviderCV>(cv => cv.ServiceProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cv => cv.ServiceProviderProfileId).IsUnique();
            entity.HasIndex(cv => cv.UploadedAt);
        });
    }
}

