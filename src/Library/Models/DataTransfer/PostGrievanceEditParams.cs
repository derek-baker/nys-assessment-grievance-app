namespace Library.Models.DataTransferObjects
{
    public class PostGrievanceEditParams: IAuthorizable
    {
        public string userName { get; set; }
        public string password { get; set; }
        public GrievanceApplication grievance { get; set; }
    }
}
