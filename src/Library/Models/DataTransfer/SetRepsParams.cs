using Library.Models.Entities;
using System.Collections.Generic;

namespace Library.Models.DataTransferObjects
{
    public class SetRepsParams
    {
        public IEnumerable<RepGroupInfo> reps { get; set; }
    }
}
