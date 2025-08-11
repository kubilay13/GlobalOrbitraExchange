using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;

namespace GlobalOrbitra.Db
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserWallet> UserWallets { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Transaction> AssetTransactions { get; set; } = null!;

    }
}
