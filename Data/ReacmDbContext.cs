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

        modelBuilder.Entity<AgentPhotographer>()
            .HasKey(ap => new { ap.AgentId, ap.PhotographerId });
        modelBuilder.Entity<AgentPhotographer>()
            .HasOne(ap => ap.Agent)
            .WithMany(a => a.AgentPhotographer)
            .HasForeignKey(ap => ap.AgentId)
             .OnDelete(DeleteBehavior.Restrict); // Avoid cascade
        modelBuilder.Entity<AgentPhotographer>()
            .HasOne(ap => ap.Photographer)
            .WithMany(a => a.AgentPhotographer)
            .HasForeignKey(ap => ap.PhotographerId)
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascade


        // delete agent would cause AgentListingCase be deleted
        // delete ListingCase would cause AgentListingCase Listing case id be null
        modelBuilder.Entity<AgentListingCase>()
            .HasKey(al => new { al.AgentId, al.ListingCaseId });
        modelBuilder.Entity<AgentListingCase>()
            .HasOne(al => al.Agent)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AgentListingCase>()
            .HasOne(al => al.ListingCase)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.ListingCaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // restrict to directly delete the listing case with referenced media assets
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id);

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
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ListingCase)
                .WithMany(l => l.MediaAssets)
                .HasForeignKey(e => e.ListingCaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
            
    }

}
