using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.ResponseDtos;
using DSH_ETL_2025.Contract.Services;
using DSH_ETL_2025.Domain.Entities;

namespace DSH_ETL_2025.Application.Services;

public class DatasetDiscoveryService : IDatasetDiscoveryService
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public DatasetDiscoveryService(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    /// <inheritdoc />
    public async Task<DatasetFullDetailsDto?> GetDatasetDetailsAsync(string identifier)
    {
        DatasetMetadata? metadata = await _repositoryWrapper.DatasetMetadata.GetMetadataAsync(identifier);

        if (metadata == null)
        {
            return null;
        }

        int metadataId = metadata.DatasetMetadataID;

        Task<DatasetGeospatialData?> geoTask = _repositoryWrapper.DatasetGeospatialData.GetSingleAsync(x => x.DatasetMetadataID == metadataId);
        Task<List<DataFile>> filesTask = _repositoryWrapper.DataFiles.GetManyAsync(x => x.DatasetMetadataID == metadataId);
        Task<List<SupportingDocument>> docsTask = _repositoryWrapper.SupportingDocuments.GetManyAsync(x => x.DatasetMetadataID == metadataId);
        Task<List<DatasetRelationship>> relsTask = _repositoryWrapper.DatasetMetadataRelationships.GetManyAsync(x => x.DatasetMetadataID == metadataId);
        Task<List<MetadataDocument>> rawTask = _repositoryWrapper.Metadata.GetManyAsync(x => x.FileIdentifier == identifier);

        await Task.WhenAll(geoTask, filesTask, docsTask, relsTask, rawTask);

        DatasetFullDetailsDto details = new DatasetFullDetailsDto
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

        if (geoTask.Result != null)
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

    /// <inheritdoc />
    public async Task<List<DatasetMetadataResultDto>> SearchDatasetsAsync(string query)
    {
        List<DatasetMetadata> searchResults = await _repositoryWrapper.DatasetMetadata.SearchMetadataAsync(query);

        return searchResults.Select(m => new DatasetMetadataResultDto
        {
            DatasetMetadataID = m.DatasetMetadataID,
            DatasetID = m.DatasetID,
            FileIdentifier = m.FileIdentifier,
            Title = m.Title,
            Description = m.Description,
            PublicationDate = m.PublicationDate,
            MetaDataDate = m.MetaDataDate
        }).ToList();
    }
}

