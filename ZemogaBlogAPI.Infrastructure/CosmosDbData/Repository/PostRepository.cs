using ZemogaBlogAPI.Core.Entities;
using ZemogaBlogAPI.Core.Enums;
using ZemogaBlogAPI.Core.Interfaces;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository
{
    public class PostRepository : CosmosDbRepository<PostItem>, IPostRepository
    {
        public override string ContainerName { get; } = "posts";

        public override string GenerateId(PostItem entity) => Guid.NewGuid().ToString();

        public PostRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            this._cosmosDbContainerFactory = cosmosDbContainerFactory ?? throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            this._container = this._cosmosDbContainerFactory.GetContainer(ContainerName)._container;
        }

        public async Task<IEnumerable<PostItem>> GetPostsByStatusAsync(Status status)
        {
            string query = $"SELECT * FROM c WHERE c.Status = {(int)status}";

            IEnumerable<PostItem> entities = await this.GetItemsAsync(query);

            return entities;
        }


        public async Task<PostItem> GetPostByIdAsync(string id)
        {
            string query = $"SELECT * FROM c WHERE c.PostId = '{id}'";

            IEnumerable<PostItem> entity = await this.GetItemsAsync(query);

            return entity.FirstOrDefault();
        }

        public async Task<IEnumerable<PostItem>> GetPostsByAuthor(string id)
        {
            string query = $"SELECT * FROM c WHERE c.AuthorId = '{id}'";

            IEnumerable<PostItem> entity = await this.GetItemsAsync(query);

            return entity;
        }

        public async Task<PostItem> UpdatePostAsync(PostItem post)
        {
            return await this.UpdateItemAsync(post);
        }

        public async Task<PostItem> CreatePostAsync(PostItem post)
        {
            post.DateCreated = DateTime.Now;

            post.id = await this.CreateItemAsync(post);

            return post;
        }
    }
}
