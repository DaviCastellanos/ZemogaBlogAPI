using ZemogaBlogAPI.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZemogaBlogAPI.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetItemsAsync(string query);

        Task CreateItemAsync(T item);

        Task <T> UpdateItemAsync(T item);

    }
}
