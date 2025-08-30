
namespace TheLightStore.Datas;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}