namespace Library.Models.DataTransferObjects
{
    public class DeleteFileParams
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string blobFullName { get; set; }
    }
}
