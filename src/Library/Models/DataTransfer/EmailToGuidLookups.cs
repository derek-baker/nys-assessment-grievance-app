using Library.Models.Email;
using System.Collections.Generic;

namespace Library.Models
{
    public class EmailToGuidLookups
    {
        public Dictionary<string, List<string>> Dict { get; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// TODO: unit test
        /// </summary>
        public static EmailToGuidLookups BuildLookups(List<SubmissionInfo> data)
        {
            var lookups = new EmailToGuidLookups();
            foreach (EmailToGuidLookupElement obj in data)
            {
                var email = obj.email.ToLower();

                if (lookups.Dict.ContainsKey(email))
                    lookups.Dict[email].Add(obj.guid);
                else
                    lookups.Dict.Add(email, new List<string>() { obj.guid });                
            }
            return lookups;
        }
    }
}
