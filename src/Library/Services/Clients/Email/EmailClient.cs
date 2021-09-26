using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Threading.Tasks;

namespace Library.Email
{
    public class EmailClient : IEmailClient
    {
        const string LINK_ROUTE = "continue-submission";
        
        public async Task SendInitialSubmissionEmail(
            string userEmail,
            string filenames,
            string guidString,
            string key,
            string hostForLink,
            string taxMapId)
        {
            // TODO: Email generation should be in a service
            string optionalLine =
                filenames.Trim().Length > 2
                    ? $"The following documents were included: {filenames} \n\n" : "";

            string emailMessage =
                $"Your grievance application was submitted for parcel: {taxMapId}. \n\n" +
                optionalLine +
                $"Please keep the following Submission ID for your records: {guidString} \n\n" +
                "If you need to submit additional supporting documentation later, " +
                "you can use the URL below in concert with the ID above to link the documents to your original submission. \n\n" +
                $"{hostForLink}/{LINK_ROUTE} \n\n";

            var data = JsonSerializer.Serialize(
                new
                {
                    emailTo = userEmail,
                    emailSubject = $"Do Not Reply - Grievance Application Submission Confirmation (NYS RP-524) - {taxMapId} - {guidString}",
                    emailMessage,
                    key
                }
            );
            throw new NotImplementedException();
            
        }

        public async Task SendSupportingDocsEmail(
            string userEmail,
            string filenames,
            string guidString,
            string apiKey,
            string hostForLink,
            string taxMapId)
        {
            string emailMessage =
                $"Your supporting documents were added to your Assessment Grievance Application for parcel: {taxMapId}. \n\n" +
                $"The following supporting documents were included: {filenames} \n\n" +
                $"Please keep the following Submission ID for your records: {guidString} \n\n" +
                "If you need to submit additional supporting documentation later, " +
                "you can use the URL below in concert with the ID above to link more documents to your original submission. \n\n" +
                $"{hostForLink}/{LINK_ROUTE} \n\n";

            var data = JsonSerializer.Serialize(
                new
                {
                    emailTo = userEmail,
                    emailSubject = $"Do Not Reply - Grievance Application Supporting Documents Submission Confirmation - {taxMapId} - {guidString}",
                    emailMessage,
                    apiKey
                }
            );
            throw new NotImplementedException();
        }

        public async void SendConflictingSubmissionsEmail(
            List<string> toList, 
            string bcc, 
            string html, 
            string subject, 
            string from,
            string apiKey)
        {
            Contract.Requires(toList != null);

            var data = JsonSerializer.Serialize(
                new
                {
                    to = toList,
                    from,
                    bcc,
                    subject = $"{subject} - {DateTime.Now}",
                    html,
                    key = apiKey
                }
            );
            throw new NotImplementedException();
        }

        public async Task SendAlertEmail(
            string userEmail,
            string apiKey,
            string subject,
            string error)
        {
            string emailMessage =
                $"AN ERROR OCCURRED! \n\n" +
                $"{subject} \n\n" +
                $"{error} \n\n";

            var data = JsonSerializer.Serialize(
                new
                {
                    emailTo = userEmail,
                    emailSubject = subject,
                    emailMessage,
                    apiKey
                }
            );

            throw new NotImplementedException();
        }
    }
}
