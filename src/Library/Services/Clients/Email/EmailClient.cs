using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Library.Email
{
    public class EmailClient : IEmailClient
    {
        // TO DO: should be config value
        const string LINK_ROUTE = "continue-submission";
        
        private readonly ISendGridClient _email;
        
        public EmailClient(ISendGridClient email)
        {
            _email = email;
        }

        public async Task SendInitialSubmissionEmail(
            string to,
            string from,
            string filenames,
            string guidString,
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

            var message = BuildEmailInputs(
                to,
                from,
                $"Do Not Reply - Grievance Application Submission Confirmation (NYS RP-524) - {taxMapId} - {guidString}",
                emailMessage);

            await _email.SendEmailAsync(message);
        }

        public async Task SendSupportingDocsEmail(
            string to,
            string from,
            string filenames,
            string guidString,
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

            var message = BuildEmailInputs(
                to,
                from,
                $"Do Not Reply - Grievance Application Supporting Documents Submission Confirmation - {taxMapId} - {guidString}",
                emailMessage);

            await _email.SendEmailAsync(message);
        }

        public void SendConflictingSubmissionsEmail(
            IEnumerable<string> toList, 
            string from,
            string html, 
            string subject)
        {
            var message = BuildEmailInputs(
                toList,
                from,
                $"{subject} - {DateTime.Now}",
                html);

            _email.SendEmailAsync(message);
        }

        public async Task SendAlertEmail(
            string to,
            string from,
            string subject,
            string error)
        {
            string emailMessage =
                $"AN ERROR OCCURRED! \n\n" +
                $"{subject} \n\n" +
                $"{error} \n\n";

            throw new NotImplementedException();
        }

        private SendGridMessage BuildEmailInputs(
            string to,
            string from,
            string emailSubject,
            string bodyPlainText)
        {
            return MailHelper.CreateSingleEmail(
                to: new EmailAddress(to),
                from: new EmailAddress(from),
                subject: emailSubject,
                plainTextContent: bodyPlainText,
                htmlContent: "");
        }

        private SendGridMessage BuildEmailInputs(
            IEnumerable<string> toList,
            string from,
            string emailSubject,
            string bodyHtml)
        {
            return MailHelper.CreateSingleEmailToMultipleRecipients(
                tos: toList.Select(to => new EmailAddress(to)).ToList(),
                from: new EmailAddress(from),
                subject: emailSubject,
                plainTextContent: "",
                htmlContent: bodyHtml);
        }
    }
}
