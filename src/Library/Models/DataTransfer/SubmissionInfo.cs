namespace Library.Models.Email
{
    public class SubmissionInfo : EmailToGuidLookupElement
    {
        public string attorney_group { get; set; }

        public SubmissionInfo() { }

        public SubmissionInfo(EmailToGuidLookupElement data)
        {
            guid = data.guid;
            email = data.email;
            tax_map_id = data.tax_map_id;
            submit_date = data.submit_date;
        }
    }
}
