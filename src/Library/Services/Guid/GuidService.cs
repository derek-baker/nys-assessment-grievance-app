using Library.Services.Clients.Database;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Library.Services.Guid
{
    public class GuidService : IGuidService
    {
        public bool TestGuidExistence(IDocumentDatabase db, DocumentDatabaseSettings dbSettings, string guidString)
        {
            var collection = db.GetCollection(dbSettings.GrievancesCollectionName);
            var documentAssociatedWithGuid = db.GetDocumentByGuid(collection, guidString);
            return documentAssociatedWithGuid != null;
        }

        public string GetNewGuid(IDocumentDatabase db, DocumentDatabaseSettings dbSettings)
        {
            bool uniqueGuidGenerated = false;
            string guid = "";

            var collection = db.GetCollection(collectionName: dbSettings.GrievancesCollectionName);
            string guidFieldNameInCollection = "guid";

            while (uniqueGuidGenerated == false)
            {
                string potentialGuid = System.Guid.NewGuid().ToString();
                var docAssociatedWithGuid = db.GetDocumentByStringField(
                    collection,
                    projection: db.BuildProjection(
                        (new List<string>() { guidFieldNameInCollection }).ToImmutableList()
                    ),
                    fieldName: guidFieldNameInCollection,
                    fieldValue: guid
                );
                if (docAssociatedWithGuid == null)
                {
                    guid = potentialGuid;
                    uniqueGuidGenerated = true;
                }
            }
            return guid;
        }
    }
}
