using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Email
{
    public interface IEmailClient
    {
        void SendConflictingSubmissionsEmail(
            List<string> toList, 
            string bcc, 
            string html, 
            string subject, 
            string from,
            string apiKey);
        
        Task SendInitialSubmissionEmail(
            string userEmail, 
            string filenames, 
            string guidString, 
            string apiKey, 
            string hostForLink, 
            string taxMapId);
        
        Task SendSupportingDocsEmail(
            string userEmail, 
            string filenames, 
            string guidString, 
            string apiKey, 
            string hostForLink, 
            string taxMapId);

        Task SendAlertEmail(
            string userEmail,
            string apiKey,
            string subject,
            string error);
    }
}