using MongoDB.Bson.Serialization.Attributes;

namespace Library.Models.DataTransfer
{
    [BsonIgnoreExtraElements]
    public class GrievanceMissingRp524
    {
        public string guid { get; set; }
        public string tax_map_id { get; set; }
    }
}
