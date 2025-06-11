using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RecamSystemApi.Models;


namespace RecamSystemApi.Data
{
    public class ReacmDbContext : IdentityDbContext<User>
    {

        public ReacmDbContext(DbContextOptions<ReacmDbContext> options) : base(options) { }

    }
}