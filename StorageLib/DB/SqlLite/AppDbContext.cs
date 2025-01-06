using Microsoft.EntityFrameworkCore;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;

namespace StorageLib.DB.SqlLite;

public class AppDbContext : DbContext
{
    public DbSet<PeerInfoModel> PeersInfo { get; set; }
    public DbSet<KnownPeersModel> PeersList { get; set; }
    public DbSet<BlockModel> Blocks { get; set; }
    public DbSet<TransactionModel> Transactions { get; set; }

    public AppDbContext() : base()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=/Users/kyrylo_shestakov/RiderProjects/Termite_2.0/StorageLib/Termite_BD.db");
        }
    }
    
}