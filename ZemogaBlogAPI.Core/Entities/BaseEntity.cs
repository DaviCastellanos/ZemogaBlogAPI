using Newtonsoft.Json;

namespace ZemogaBlogAPI.Core.Entities
{
    public abstract class BaseEntity
    {
        [JsonProperty(PropertyName = "id")]
        public virtual string id { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
