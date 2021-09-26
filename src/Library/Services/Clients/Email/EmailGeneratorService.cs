using Library.Models.Email;
using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace Library.Services.Email
{
    public static class EmailContentGeneratorService
    {
        public static string FormatSubjectLine(string topic)
        {
            return $"Do Not Reply - {topic} - {DateTime.Now.ToShortDateString()}";
        }

        /// <summary>
        /// TODO: Sort list of emails by date of submission
        /// </summary>
        public static string GenerateConflictingSubmissionsHtml(
            ImmutableList<string> emailList,
            string taxMapId
        )
        {
            Contract.Requires(emailList != null);
            string emailsWithLineBreaks = "";
            emailList.ForEach(
                e =>
                {
                    emailsWithLineBreaks += $"{e} <br>";
                }
            );
            string msg = @$"
                <p>
                <b>Greetings,</b>
                <br>
                <br>
                The purpose of this email is to notify you that multiple parties have submitted 
                Assessment Grievance Applications (NYS RP-524) for the parcel identified below, 
                for which you also submitted a RP-524.
                <br>
                <br>                
                <u>Parcel Identifier: </u> 
                <br>
                {taxMapId}
                <br>
                <br>
                <u>Submitting parties: </u> 
                <br>
                {emailsWithLineBreaks}
                <br>
                <br>
                <b>Please do not reply to this email.</b>
                </p>
            ";
            return msg;
        }
    }
}
