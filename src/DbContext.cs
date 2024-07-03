using Microsoft.EntityFrameworkCore;
namespace Zggff.MaiPractice;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pet> Pets { get; set; }
}