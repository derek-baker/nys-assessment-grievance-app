using Library.Services.Clients.Database;

namespace Library.Services.Guid
{
    public interface IGuidService
    {
        bool TestGuidExistence(IDocumentDatabase db, DocumentDatabaseSettings dbSettings, string guidString);

        string GetNewGuid(IDocumentDatabase db, DocumentDatabaseSettings dbSettings);
    }
}
