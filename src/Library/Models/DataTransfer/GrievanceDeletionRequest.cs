namespace Library.Models.DataTransferObjects
{
    public class GrievanceDeletionRequest: IAuthorizable
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string grievanceId { get; set; }
    }
}
