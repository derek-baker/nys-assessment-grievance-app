using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Email
{
    public interface IEmailClient
    {
        /// <summary>
        /// Intended to be used in a fire-and-forget manner, as this message is not critical,
        /// and should not block/slow the user action
        /// </summary>
        void SendConflictingSubmissionsEmail(
            IEnumerable<string> toList,
            string from,
            string html,
            string subject);


        Task SendInitialSubmissionEmail(
            string to,
            string from,
            string filenames, 
            string guidString,
            string hostForLink, 
            string taxMapId);
        
        Task SendSupportingDocsEmail(
            string to,
            string from,
            string filenames, 
            string guidString,
            string hostForLink, 
            string taxMapId);

        Task SendAlertEmail(
            string to,
            string from,
            string subject,
            string error);

        Task SendUserCreationEmail(
            string to,
            string password,
            string loginUrl);
    }
}