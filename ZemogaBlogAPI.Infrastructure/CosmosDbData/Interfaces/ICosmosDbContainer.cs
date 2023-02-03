using Microsoft.Azure.Cosmos;

namespace ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces
{
    public interface ICosmosDbContainer
    {
        Container _container { get; }
    }
}
