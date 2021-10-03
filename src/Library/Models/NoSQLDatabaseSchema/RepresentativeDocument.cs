namespace Library.Models.NoSQLDatabaseSchema
{
    public class RepresentativesDocument
    {
        public static RepresentativesDocumentFields Fields { get; } = new RepresentativesDocumentFields();
    }

    public class RepresentativesDocumentFields
    {
        public string Representatives { get; } = "Representatives";
    }
}
