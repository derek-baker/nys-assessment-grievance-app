using Library.Database;
using Library.Models;

namespace Library.Services.Guid
{
    public interface IGuidService
    {
        bool TestGuidExistence(IDocumentDatabase db, DocumentDatabaseSettings dbSettings, string guidString);

        string GetNewGuid(IDocumentDatabase db, DocumentDatabaseSettings dbSettings);
    }
}
