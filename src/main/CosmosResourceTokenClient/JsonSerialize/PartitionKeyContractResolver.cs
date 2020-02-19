using System;
using System.Reflection;
using CosmosResourceTokenClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosResourceTokenClient.JsonSerialize
{
    // https://stackoverflow.com/a/33290710/4140832
    public class PartitionKeyContractResolver<T> : DefaultContractResolver
    {
        private readonly string _partitionKeyHeaderName;

        public PartitionKeyContractResolver(string partitionKeyHeaderName)
        {
            _partitionKeyHeaderName = partitionKeyHeaderName;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(CosmosItem<T>))
            {
                if (property.PropertyName == "PartitionKeyHeaderNameForCosmosItem")
                {
                    property.PropertyName = _partitionKeyHeaderName;
                }
            }

            return property;
        }
    }
}
