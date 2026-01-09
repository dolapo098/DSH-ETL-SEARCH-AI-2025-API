using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Infrastructure.Repositories;

namespace DSH_ETL_2025.Infrastructure.DataAccess;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly EtlDbContext _dbContext;
    private IDatasetMetadataRepository? _datasetMetadata;
    private IDatasetRepository? _datasets;
    private IMetadataRepository? _metadata;
    private IDatasetGeospatialDataRepository? _datasetGeospatialData;
    private IDataFileRepository? _dataFiles;
    private ISupportingDocumentRepository? _supportingDocuments;
    private IDatasetMetadataRelationshipRepository? _datasetMetadataRelationships;
    private IDatasetSupportingDocumentQueueRepository? _datasetSupportingDocumentQueues;

    public RepositoryWrapper(EtlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public IDatasetMetadataRepository DatasetMetadata => 
        _datasetMetadata ??= new DatasetMetadataRepository(_dbContext);

    /// <inheritdoc />
    public IDatasetRepository Datasets => 
        _datasets ??= new DatasetRepository(_dbContext);

    /// <inheritdoc />
    public IMetadataRepository Metadata => 
        _metadata ??= new MetadataRepository(_dbContext);

    /// <inheritdoc />
    public IDatasetGeospatialDataRepository DatasetGeospatialData => 
        _datasetGeospatialData ??= new DatasetGeospatialDataRepository(_dbContext);

    /// <inheritdoc />
    public IDataFileRepository DataFiles => 
        _dataFiles ??= new DataFileRepository(_dbContext);

    /// <inheritdoc />
    public ISupportingDocumentRepository SupportingDocuments => 
        _supportingDocuments ??= new SupportingDocumentRepository(_dbContext);

    /// <inheritdoc />
    public IDatasetMetadataRelationshipRepository DatasetMetadataRelationships => 
        _datasetMetadataRelationships ??= new DatasetMetadataRelationshipRepository(_dbContext);

    /// <inheritdoc />
    public IDatasetSupportingDocumentQueueRepository DatasetSupportingDocumentQueues => 
        _datasetSupportingDocumentQueues ??= new DatasetSupportingDocumentQueueRepository(_dbContext);

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await action();
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch ( Exception )
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
