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
                .WithOne(u=> u.Photographer)
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
             .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

            entity
            .HasOne(ap => ap.Photographer)
            .WithMany(a => a.AgentPhotographer)
            .HasForeignKey(ap => ap.PhotographerId)
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascade

        });

        modelBuilder.Entity<AgentListingCase>((entity) =>
        {
            entity.HasKey(al => new { al.AgentId, al.ListingCaseId });
            entity
            .HasOne(al => al.Agent)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
            entity
            .HasOne(al => al.ListingCase)
            .WithMany(a => a.AgentListingCases)
            .HasForeignKey(al => al.ListingCaseId)
            .OnDelete(DeleteBehavior.Restrict);
        });

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
