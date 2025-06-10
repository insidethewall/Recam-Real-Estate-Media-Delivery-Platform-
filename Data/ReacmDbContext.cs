using Microsoft.EntityFrameworkCore;


namespace ReacmSystemApi.Data
{
    public class ReacmDbContext : DbContext
    {
        public ReacmDbContext(DbContextOptions<ReacmDbContext> options) : base(options) { }

    }
}