using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Console.EF.Cosmos.Model
{
    public class Person
    {
        [Key]
        [JsonPropertyName("id")]
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }


        public string PartitionKey { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
