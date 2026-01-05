using DSH_ETL_2025.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DSH_ETL_2025.Infrastructure.DataAccess;

public class EtlDbContext : DbContext
{
    private readonly ILogger<EtlDbContext>? _logger;

    public EtlDbContext(DbContextOptions<EtlDbContext> options, ILogger<EtlDbContext>? logger = null) : base(options)
    {
        _logger = logger;
        
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
            _logger?.LogInformation(ex,
                "Database initialization notice");
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
