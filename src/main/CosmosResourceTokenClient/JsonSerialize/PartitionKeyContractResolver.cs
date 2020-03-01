using System.Reflection;
using CosmosResourceTokenClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosResourceTokenClient.JsonSerialize
{
    // https://stackoverflow.com/a/33290710/4140832
    [Preserve(AllMembers = true)]
    public class PartitionKeyContractResolver<T> : DefaultContractResolver
    {
        private readonly string _partitionKeyHeaderName;
        private readonly string _replaceWithString;

        public PartitionKeyContractResolver(string partitionKeyHeaderName, string replaceWithString)
        {
            _partitionKeyHeaderName = partitionKeyHeaderName;
            _replaceWithString = replaceWithString;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(CosmosItem<T>))
            {
                if (property.PropertyName == _replaceWithString)
                {
                    property.PropertyName = _partitionKeyHeaderName;
                }
            }

            return property;
        }
    }
}
