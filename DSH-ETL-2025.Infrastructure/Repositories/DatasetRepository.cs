using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DSH_ETL_2025.Infrastructure.Repositories;

public class DatasetRepository : IDatasetRepository
{
    private readonly EtlDbContext _dbContext;

    public DatasetRepository(EtlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<int> GetTotalDatasetsCountAsync()
    {
        return await _dbContext.DatasetMetadatas.CountAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetTotalProvidersCountAsync()
    {
        var count = await _dbContext.DataFiles
            .Where(f => !string.IsNullOrEmpty(f.DownloadUrl))
            .Select(f => f.DownloadUrl)
            .Distinct()
            .CountAsync();

        return count;
    }

    /// <inheritdoc />
    public async Task<DatasetFullDetailsDto?> GetDatasetFullDetailsAsync(string identifier)
    {
        var metadata = await _dbContext.DatasetMetadatas
            .FirstOrDefaultAsync(m => m.FileIdentifier == identifier);

        if ( metadata == null )
        {
            return null;
        }

        var metadataId = metadata.DatasetMetadataID;

        var geoTask = _dbContext.DatasetGeospatialDatas
            .FirstOrDefaultAsync(x => x.DatasetMetadataID == metadataId);

        var filesTask = _dbContext.DataFiles
            .Where(x => x.DatasetMetadataID == metadataId)
            .ToListAsync();

        var docsTask = _dbContext.SupportingDocuments
            .Where(x => x.DatasetMetadataID == metadataId)
            .ToListAsync();

        var relsTask = _dbContext.DatasetRelationships
            .Where(x => x.DatasetMetadataID == metadataId)
            .ToListAsync();

        var rawTask = _dbContext.MetadataDocuments
            .Where(x => x.FileIdentifier == identifier)
            .ToListAsync();

        await Task.WhenAll(geoTask, filesTask, docsTask, relsTask, rawTask);

        var details = new DatasetFullDetailsDto
        {
            DatasetMetadata = new DatasetMetadataResultDto
            {
                DatasetMetadataID = metadata.DatasetMetadataID,
                DatasetID = metadata.DatasetID,
                FileIdentifier = metadata.FileIdentifier,
                Title = metadata.Title,
                Description = metadata.Description,
                PublicationDate = metadata.PublicationDate,
                MetaDataDate = metadata.MetaDataDate,
                CreatedAt = metadata.CreatedAt,
                UpdatedAt = metadata.UpdatedAt
            },
            DataFiles = filesTask.Result.Select(f => new DataFileDto
            {
                DataFileID = f.DataFileID,
                Title = f.Title,
                Description = f.Description,
                Type = f.Type,
                FileType = f.FileType,
                DownloadUrl = f.DownloadUrl
            }).ToList(),
            SupportingDocuments = docsTask.Result.Select(d => new SupportingDocumentDto
            {
                SupportingDocumentID = d.SupportingDocumentID,
                Title = d.Title,
                DownloadUrl = d.DownloadUrl,
                DocumentType = d.DocumentType
            }).ToList(),
            Relationships = relsTask.Result.Select(r => new DatasetRelationshipDto
            {
                DatasetRelationshipID = r.DatasetRelationshipID,
                DatasetID = r.DatasetID,
                RelationshipType = r.RelationshipType,
                RelationshipUri = r.RelationshipUri
            }).ToList(),
            RawDocuments = rawTask.Result.Select(d => new RawMetadataDocumentDto
            {
                MetadataDocumentID = d.MetadataDocumentID,
                DocumentType = d.DocumentType.ToString(),
                RawDocument = d.RawDocument
            }).ToList()
        };

        if ( geoTask.Result != null )
        {
            details.GeospatialData = new DatasetGeospatialDataDto
            {
                DatasetGeospatialDataID = geoTask.Result.DatasetGeospatialDataID,
                Abstract = geoTask.Result.Abstract,
                TemporalExtentStart = geoTask.Result.TemporalExtentStart,
                TemporalExtentEnd = geoTask.Result.TemporalExtentEnd,
                BoundingBox = geoTask.Result.BoundingBox,
                Contact = geoTask.Result.Contact,
                MetadataStandard = geoTask.Result.MetadataStandard,
                StandardVersion = geoTask.Result.StandardVersion,
                Status = geoTask.Result.Status
            };
        }

        return details;
    }
}
