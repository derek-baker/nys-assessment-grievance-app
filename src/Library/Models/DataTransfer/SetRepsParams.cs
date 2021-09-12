using System.Collections.Generic;

namespace Library.Models.DataTransferObjects
{
    public class SetRepsParams: UserAuthInfo
    {
        public IEnumerable<RepGroupInfo> reps { get; set; }
    }
}
