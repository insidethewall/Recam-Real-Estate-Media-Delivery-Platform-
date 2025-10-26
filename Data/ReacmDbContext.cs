using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RecamSystemApi.Models;


namespace RecamSystemApi.Data;

public class ReacmDbContext : IdentityDbContext<User>
{
    public DbSet<ListingCase> ListingCases { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<CaseContact> CaseContacts { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Photographer> PhotographyCompanies { get; set; }
    public DbSet<AgentPhotographer> AgentPhotographers { get; set; }
    public DbSet<AgentListingCase> AgentListingCases { get; set; }
    public ReacmDbContext(DbContextOptions<ReacmDbContext> options) : base(options)
    {


    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.User)
                .WithOne(u =>u.Agent)
                .HasForeignKey<Agent>(a => a.Id)
                .OnDelete(DeleteBehavior.Cascade); // When User is deleted, Agent is deleted

            entity.Property(a => a.CompanyName).IsRequired();
            entity.Property(a => a.AgentFirstName).IsRequired();
            entity.Property(a => a.AgentLastName).IsRequired();
        });

        modelBuilder.Entity<Photographer>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.User)
                .WithOne(u => u.Photographer)
                .HasForeignKey<Photographer>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade); // When User is deleted, Photographer is deleted

            entity.Property(p => p.CompanyName).IsRequired();
            entity.Property(p => p.PhotographerFirstName).IsRequired();
            entity.Property(p => p.PhotographerLastName).IsRequired();

        });

        modelBuilder.Entity<AgentPhotographer>(entity =>
        {
            entity.HasKey(ap => new { ap.AgentId, ap.PhotographerId });

            entity
            .HasOne(ap => ap.Agent)
            .WithMany(a => a.AgentPhotographer)
            .HasForeignKey(ap => ap.AgentId)
             .OnDelete(DeleteBehavior.Cascade); // When Agent is deleted, related AgentPhotographer entries are deleted

            entity
            .HasOne(ap => ap.Photographer)
            .WithMany(a => a.AgentPhotographer)
            .HasForeignKey(ap => ap.PhotographerId)
            .OnDelete(DeleteBehavior.Cascade);  // When Photographer is deleted, related AgentPhotographer entries are deleted

        });

        modelBuilder.Entity<AgentListingCase>((entity) =>
        {
            entity.HasKey(al => new { al.AgentId, al.ListingCaseId });
            entity
            .HasOne(al => al.Agent)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
            entity
            .HasOne(al => al.ListingCase)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.ListingCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        });
       

        modelBuilder.Entity<ListingCase>((entity) =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Street)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Postcode)
                .IsRequired();

            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9,6)");

            entity.Property(e => e.Price)
                .IsRequired();

            entity.Property(e => e.Bedrooms)
                .IsRequired();

            entity.Property(e => e.Bathrooms)
                .IsRequired();

            entity.Property(e => e.Garages)
                .IsRequired();

            entity.Property(e => e.FloorArea)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(e => e.PropertyType)
                .IsRequired();

            entity.Property(e => e.SaleCategory)
                .IsRequired();

            entity.Property(e => e.ListcaseStatus)
                .HasDefaultValue(ListcaseStatus.Created);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.ListingCases)
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of User if ListingCases exist
        });

        
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id);;

            entity.Property(e => e.FileName).IsRequired()
                .HasMaxLength(255); 

            entity.Property(e => e.FilePath).IsRequired()
                .HasMaxLength(1000); 

            entity.Property(e => e.MediaType).IsRequired();

            entity.Property(e => e.UserId).IsRequired();

            entity.Property(e => e.ListingCaseId).IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u=>u.MediaAssets)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // When User is deleted, related MediaAssets are deleted

            entity.HasOne(e => e.ListingCase)
                .WithMany(l => l.MediaAssets)
                .HasForeignKey(e => e.ListingCaseId)
                .OnDelete(DeleteBehavior.Cascade); // When ListingCase is deleted, related MediaAssets are deleted
        });
            
    }

}
