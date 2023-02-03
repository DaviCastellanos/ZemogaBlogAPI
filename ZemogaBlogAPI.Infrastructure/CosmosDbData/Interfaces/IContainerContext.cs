using ZemogaBlogAPI.Core.Entities;
using Microsoft.Azure.Cosmos;

namespace ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces
{
    public interface IContainerContext<T> where T : BaseEntity
    {
        string ContainerName { get; }
        string GenerateId(T entity);
    }
}
