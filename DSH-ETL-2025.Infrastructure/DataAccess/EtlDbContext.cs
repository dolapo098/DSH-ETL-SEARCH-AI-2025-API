using DSH_ETL_2025.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.DataAccess;

public class EtlDbContext : DbContext
{
    public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
    {
        if ( Database.IsRelational() )
        {
            Database.OpenConnection();
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }
    }

    public DbSet<DatasetMetadata> DatasetMetadatas { get; set; }

    public DbSet<DataFile> DataFiles { get; set; }

    public DbSet<DatasetGeospatialData> DatasetGeospatialDatas { get; set; }

    public DbSet<DatasetRelationship> DatasetRelationships { get; set; }

    public DbSet<DatasetSupportingDocumentQueue> DatasetSupportingDocumentQueues { get; set; }

    public DbSet<MetadataDocument> MetadataDocuments { get; set; }

    public DbSet<SupportingDocument> SupportingDocuments { get; set; }

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
