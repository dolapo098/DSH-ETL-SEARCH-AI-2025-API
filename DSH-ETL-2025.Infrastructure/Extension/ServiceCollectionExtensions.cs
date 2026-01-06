using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Extractors;
using DSH_ETL_2025.Contract.Parsers;
using DSH_ETL_2025.Contract.Processors;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Infrastructure.DataAccess;
using DSH_ETL_2025.Infrastructure.Extractors;
using DSH_ETL_2025.Infrastructure.Extractors.Formats;
using DSH_ETL_2025.Infrastructure.Parsers;
using DSH_ETL_2025.Infrastructure.Processors;
using DSH_ETL_2025.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DSH_ETL_2025.Infrastructure.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EtlSettings>(config.GetSection(EtlSettings.SectionName));

            services.AddDbContext<EtlDbContext>(options =>
                options.UseSqlite(config.GetConnectionString("DefaultConnection")));
            
            services.AddScoped<IDatasetMetadataRepository, DatasetMetadataRepository>();

            services.AddScoped<IMetadataRepository, MetadataRepository>();

            services.AddScoped<IDatasetGeospatialDataRepository, DatasetGeospatialDataRepository>();

            services.AddScoped<IDataFileRepository, DataFileRepository>();

            services.AddScoped<ISupportingDocumentRepository, SupportingDocumentRepository>();

            services.AddScoped<IDatasetMetadataRelationshipRepository, DatasetMetadataRelationshipRepository>();

            services.AddScoped<IDatasetSupportingDocumentQueueRepository, DatasetSupportingDocumentQueueRepository>();

            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            services.AddScoped<IJsonMetadataParser, JsonMetadataParser>();

            services.AddScoped<IIso19115Parser, Iso19115Parser>();

            services.AddScoped<IDocumentFormatExtractor, JsonFormatExtractor>();

            services.AddScoped<IDocumentFormatExtractor, Iso19115FormatExtractor>();

            services.AddScoped<IDocumentFormatExtractor, JsonLdFormatExtractor>();

            services.AddScoped<IDocumentFormatExtractor, TurtleFormatExtractor>();

            services.AddScoped<IMetadataExtractor, MetadataExtractor>();

            services.AddScoped<IDocumentProcessor, JsonDocumentProcessor>();

            services.AddScoped<IDocumentProcessor, Iso19115DocumentProcessor>();

            services.AddScoped<IDocumentProcessor, JsonLdDocumentProcessor>();

            services.AddScoped<IDocumentProcessor, TurtleDocumentProcessor>();

            return services;
        }
    }
}
