namespace Library.Models.DataTransferObjects
{
    public class GetGrievanceFileParams
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string SubmissionGuid { get; set; }
    }
}
