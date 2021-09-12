namespace Library.Models
{
    public class EmailToGuidLookupElement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
        public string guid { get; set; }
        public string email { get; set; }
        public string tax_map_id { get; set; }
        public string submit_date { get; set; }
    }
}
