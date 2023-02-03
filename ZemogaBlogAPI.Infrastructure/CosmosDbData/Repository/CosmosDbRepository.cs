using ZemogaBlogAPI.Core.Entities;
using ZemogaBlogAPI.Core.Interfaces;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces;
using Microsoft.Azure.Cosmos;

namespace ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository
{
    public abstract class CosmosDbRepository<T> : IRepository<T>, IContainerContext<T> where T : BaseEntity
    {
        public abstract string ContainerName { get; }

        public abstract string GenerateId(T entity);

        public virtual PartitionKey ResolveAuditPartitionKey(string entityId) => new PartitionKey($"{entityId.Split(':')[0]}:{entityId.Split(':')[1]}");

        private readonly ICosmosDbContainerFactory _cosmosDbContainerFactory;

        private readonly Microsoft.Azure.Cosmos.Container _container;

        public CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            this._cosmosDbContainerFactory = cosmosDbContainerFactory ?? throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            this._container = this._cosmosDbContainerFactory.GetContainer(ContainerName)._container;
        }

        public async Task CreateItemAsync(T item)
        {
            item.id = GenerateId(item);
            try
            {
                await _container.CreateItemAsync<T>(item, new PartitionKey(item.id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
        {
            FeedIterator<T> resultSetIterator = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            List<T> results = new List<T>();
            while (resultSetIterator.HasMoreResults)
            {
                FeedResponse<T> response = await resultSetIterator.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task <T> UpdateItemAsync(T item)
        {
            return await this._container.UpsertItemAsync<T>(item, new PartitionKey(item.id));
        }

    }
}
