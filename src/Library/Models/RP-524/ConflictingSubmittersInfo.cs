using System.Collections.Generic;

namespace Library.Models
{
    public class ConflictingSubmittersInfo
    {
        public List<GrievanceApplication> ConflictingApplications { get; }
        public string EmailSubmitters { get; }

        public ConflictingSubmittersInfo(
            List<GrievanceApplication> apps,
            string submitters
        )
        {
            ConflictingApplications = apps;
            EmailSubmitters = submitters;
        }
    }
}
