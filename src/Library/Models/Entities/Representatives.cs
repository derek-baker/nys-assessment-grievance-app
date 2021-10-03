using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Library.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class RepGroupInfos
    {
        public IEnumerable<RepGroupInfo> Representatives { get; set; }
    }

    public class RepGroupInfo
    {
        [JsonProperty("GroupNo")]
        public string GroupNo { get; set; }

        [JsonProperty("GroupName1")]
        public string GroupName1 { get; set; }

        [JsonProperty("ContactName")]
        public string ContactName { get; set; }

        [JsonProperty("Address1")]
        public string Address1 { get; set; }

        [JsonProperty("Address2")]
        public string Address2 { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("ZipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("Phone1")]
        public string Phone1 { get; set; }

        [JsonProperty("Phone2")]
        public string Phone2 { get; set; }

        [JsonProperty("FAX1")]
        public string FAX1 { get; set; }

        [JsonProperty("FAX2")]
        public string FAX2 { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }
    }
}
