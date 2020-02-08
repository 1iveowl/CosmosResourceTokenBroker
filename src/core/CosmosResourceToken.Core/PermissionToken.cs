﻿using Newtonsoft.Json;

namespace CosmosResourceToken.Core
{
    public class PermissionToken : IPermissionToken
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
        [JsonProperty(PropertyName = "expires")]
        public int Expires { get; set; }
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }
    }
}
