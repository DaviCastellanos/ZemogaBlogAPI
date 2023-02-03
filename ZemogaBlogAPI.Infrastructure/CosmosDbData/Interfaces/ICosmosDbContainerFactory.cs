using System.Threading.Tasks;

namespace ZemogaBlogAPI.Infrastructure.CosmosDbData.Interfaces
{
    public interface ICosmosDbContainerFactory
    {
        ICosmosDbContainer GetContainer(string containerName);
    }
}
