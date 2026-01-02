using DSH_ETL_2025.Application.Services;
using DSH_ETL_2025.Contract.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DSH_ETL_2025.Application.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEtlService, EtlService>();

        services.AddScoped<IMetadataResourceService, MetadataResourceService>();

        services.AddScoped<IDatasetDiscoveryService, DatasetDiscoveryService>();

        return services;
    }
}

