using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Models;


namespace RecamSystemApi.Data
{
    public class ReacmDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public ReacmDbContext(DbContextOptions<ReacmDbContext> options) : base(options) { }

    }
}