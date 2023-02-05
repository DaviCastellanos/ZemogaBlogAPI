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
    public class CommentRepository : CosmosDbRepository<CommentItem>, ICommentRepository
    {
        public override string ContainerName { get; } = "comments";

        public override string GenerateId(CommentItem entity) => Guid.NewGuid().ToString();

        public CommentRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            this._cosmosDbContainerFactory = cosmosDbContainerFactory ?? throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            this._container = this._cosmosDbContainerFactory.GetContainer(ContainerName)._container;
        }

        public async Task<IEnumerable<CommentItem>> GetCommentsByPostIdAsync(string id, bool includeReview)
        {

            string query = $"SELECT * FROM c Where c.PostId = '{id}'";

            IEnumerable<CommentItem> entities = await this.GetItemsAsync(query);


            return entities.Where(x => x.IsReview == includeReview);

        }

        public async Task<CommentItem> CreateCommentAsync(CommentItem comment)
        {
            comment.DateCreated = DateTime.Now;

            comment.id = await this.CreateItemAsync(comment);

            return comment;
        }
    }
}
