using DSH_ETL_2025.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DSH_ETL_2025.Infrastructure.DataAccess;

public class EtlDbContext : DbContext
{
    public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
    {
        try
        {
            if ( Database != null && Database.IsRelational() )
            {
                Database.OpenConnection();

                Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            }
        }
        catch ( Exception ex )
        {
            Console.WriteLine($"Database initialization notice: {ex.Message}");
        }
    }

    public virtual DbSet<DatasetMetadata> DatasetMetadatas { get; set; } = null!;

    public virtual DbSet<DataFile> DataFiles { get; set; } = null!;

    public virtual DbSet<DatasetGeospatialData> DatasetGeospatialDatas { get; set; } = null!;

    public virtual DbSet<DatasetRelationship> DatasetRelationships { get; set; } = null!;

    public virtual DbSet<DatasetSupportingDocumentQueue> DatasetSupportingDocumentQueues { get; set; } = null!;

    public virtual DbSet<MetadataDocument> MetadataDocuments { get; set; } = null!;

    public virtual DbSet<SupportingDocument> SupportingDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DatasetMetadata>().HasKey(m => m.DatasetMetadataID);

        modelBuilder.Entity<DataFile>().HasKey(f => f.DataFileID);

        modelBuilder.Entity<DatasetGeospatialData>().HasKey(g => g.DatasetGeospatialDataID);

        modelBuilder.Entity<DatasetRelationship>().HasKey(r => r.DatasetRelationshipID);

        modelBuilder.Entity<DatasetSupportingDocumentQueue>().HasKey(q => q.DatasetSupportingDocumentQueueID);

        modelBuilder.Entity<MetadataDocument>().HasKey(d => d.MetadataDocumentID);

        modelBuilder.Entity<SupportingDocument>().HasKey(s => s.SupportingDocumentID);

        base.OnModelCreating(modelBuilder);
    }
}
