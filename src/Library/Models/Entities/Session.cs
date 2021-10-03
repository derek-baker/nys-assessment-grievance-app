using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Library.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
