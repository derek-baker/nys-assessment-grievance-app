using CsvHelper;
using Library.Models;
using Library.Models.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services.Csv
{
    public class CsvGeneratorService : ICsvGeneratorService
    {
        public async Task<byte[]> Generate(IEnumerable<GrievanceApplication> data)
        {
            using var memStream = new MemoryStream();
            using var writer = new StreamWriter(memStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var preppedData = data.Select(d => new GrievanceCsvRow(d));
            await csv.WriteRecordsAsync(preppedData);
            await csv.FlushAsync();
            return memStream.ToArray();
        }

        public class GrievanceCsvRow
        {
            public string TaxMapID { get; set; }
            public string GrievanceID { get; set; }
            public string Complainant { get; set; }
            public string SubmitterEmail { get; set; }
            public string SubmitDate { get; set; }
            public string ComplainantMailingAddress { get; set; }
            public string CoOpUnitNum { get; set; }
            public string Reason { get; set; }
            public string Notes { get; set; }
            public string RepGroupName { get; set; }
            public string RepGroupNo { get; set; }
            public string RepContact { get; set; }
            public string RepEmail { get; set; }
            public string RepPhone1 { get; set; }
            public string RepPhone2 { get; set; }
            public string RepAddress1 { get; set; }
            public string RepAddress2 { get; set; }
            public string RepCity { get; set; }
            public string RepState { get; set; }
            public string RepZip { get; set; }
            public string RepFax1 { get; set; }
            public string RepFax2 { get; set; }
            public bool HearingRequested { get; set; }
            public bool HearingCompleted { get; set; }
            public string ComplaintType { get; set; }
            public string ProposedValue { get; set; }
            public string CreatedBy { get; set; }
            public bool BarReviewStatus { get; set; }
            public string BarReviewDate { get; set; }
            public bool Downloaded { get; set; }
            public string DownloadDate { get; set; }
            public bool ConflictOfInterest { get; set; }
            public bool IncludesResQuestionnaire { get; set; }
            public bool IncludesComQuestionnaire { get; set; }
            public bool IncludesAuthLetter { get; set; }
            public bool IncludesIncomeExpenseForms { get; set; }
            public bool IncludesIncomeExpenseExclusion { get; set; }
            public bool IncludesSupportingDocs { get; set; }
            public string SignatureType { get; set; }
            public string RP525Tentative { get; set; }
            public string RP525IsReduced { get; set; }
            public string RP525ReducedTo { get; set; }

            public GrievanceCsvRow(GrievanceApplication grievance)
            {
                var attorneyInfo = grievance.attorney_data_raw != null
                    ? JsonConvert.DeserializeObject<RepGroupInfo>(grievance.attorney_data_raw)
                    : new RepGroupInfo();

                TaxMapID = Clean(grievance.tax_map_id);
                GrievanceID = Clean(grievance.guid);
                Complainant = Clean(grievance.complainant);
                SubmitterEmail = Clean(grievance.email);
                SubmitDate = Clean(grievance.submit_date);

                RepGroupName = Clean(attorneyInfo?.GroupName1);
                RepGroupNo = Clean(attorneyInfo?.GroupNo);
                RepContact = Clean(attorneyInfo?.ContactName);
                RepEmail = Clean(attorneyInfo?.Email);
                RepPhone1 = Clean(attorneyInfo?.Phone1);
                RepPhone2 = Clean(attorneyInfo?.Phone2);
                RepAddress1 = Clean(attorneyInfo?.Address1);
                RepAddress2 = Clean(attorneyInfo?.Address2);
                RepCity = Clean(attorneyInfo?.City);
                RepState = Clean(attorneyInfo?.State);
                RepZip = Clean(attorneyInfo?.ZipCode);
                RepFax1 = Clean(attorneyInfo?.FAX1);
                RepFax2 = Clean(attorneyInfo?.FAX2);

                ComplainantMailingAddress = Clean(grievance.complainant_mail_address);
                CoOpUnitNum = Clean(grievance.co_op_unit_num);
                Reason = Clean(grievance.reason);
                Notes = Clean(grievance.notes);

                HearingRequested = grievance.requested_personal_hearing;
                HearingCompleted = grievance.completed_personal_hearing;
                ComplaintType = Clean(grievance.complaint_type);
                ProposedValue = grievance.proposed_value;
                CreatedBy = Clean(grievance.creator_name);

                BarReviewStatus = grievance.barReviewed;
                BarReviewDate = Clean(grievance.barReviewDate);
                Downloaded = grievance.downloaded;
                DownloadDate = Clean(grievance.download_date);

                ConflictOfInterest = grievance.includes_conflict;
                IncludesResQuestionnaire = grievance.includes_res_questionnaire;
                IncludesComQuestionnaire = grievance.includes_com_questionnaire;
                IncludesAuthLetter = grievance.includes_letter_of_auth;

                IncludesIncomeExpenseForms = grievance.includes_income_expense_forms;
                IncludesIncomeExpenseExclusion = grievance.includes_income_expense_exclusion;
                IncludesSupportingDocs = grievance.includes_supporting_docs;

                SignatureType = Clean(grievance.five_signature_type);

                RP525Tentative = grievance.nys_rp525_tentative;
                RP525IsReduced = Clean(grievance.nys_rp525_is_reduced);
                RP525ReducedTo = grievance.nys_rp525_is_reduced_value;
            }

            private string Clean(string toClean)
            {
                if (string.IsNullOrEmpty(toClean))
                    return toClean;
                        
                return toClean
                    .Replace(",", " ");
            }
        }
    }
}
