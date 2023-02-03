using ZemogaBlogAPI.Infrastructure.AppSettings;
using ZemogaBlogAPI.Infrastructure.CosmosDbData;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ZemogaBlogAPI.Infrastructure.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services,
                                                     string endpointUrl,
                                                     string primaryKey,
                                                     string databaseName,
                                                     List<ContainerInfo> containers)
        {
            Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(endpointUrl, primaryKey);
            CosmosDbContainerFactory cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName, containers);

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }

    }
}
