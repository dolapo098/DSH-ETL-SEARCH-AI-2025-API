using DSH_ETL_2025.Contract.Configurations;
using DSH_ETL_2025.Contract.DataAccess;
using DSH_ETL_2025.Contract.Repositories;
using DSH_ETL_2025.Infrastructure.DataAccess;
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
                options.UseSqlite(config.GetConnectionString("DefaultConnection")
                    ?? "Data Source=etl_database.db"));

            services.AddScoped<IDatasetMetadataRepository, DatasetMetadataRepository>();

            services.AddScoped<IMetadataRepository, MetadataRepository>();

            services.AddScoped<IDatasetGeospatialDataRepository, DatasetGeospatialDataRepository>();

            services.AddScoped<IDataFileRepository, DataFileRepository>();

            services.AddScoped<ISupportingDocumentRepository, SupportingDocumentRepository>();

            services.AddScoped<IDatasetMetadataRelationshipRepository, DatasetMetadataRelationshipRepository>();

            services.AddScoped<IDatasetSupportingDocumentQueueRepository, DatasetSupportingDocumentQueueRepository>();

            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            return services;
        }
    }
}
