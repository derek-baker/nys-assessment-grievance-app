namespace Library.Models.DataTransferObjects
{
    /// <summary>
    /// This is used (successfully) by two workflows. 
    /// TODO: Refactor to create two classes due to reason above.
    /// </summary>
    public class GrievanceHearingIsCompletedUpdateParams : IAuthorizable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
        public string guid { get; set; }
        public bool isHearingCompleted { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }
}
