using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Library.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Settings
    {
        public UserSettings UserSettings { get; set; }
    }

    public class UserSettings
    {
        public decimal? LevelOfAssessment { get; set; }
        public int? Year { get; set; }
        public DateTime? SubmissionsStartDate { get; set; }
        public DateTime? SubmissionsEndDate { get; set; }
        public DateTime? SupportingDocsEndDate { get; set; }
    }
}
