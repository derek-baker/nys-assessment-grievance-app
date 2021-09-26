namespace Library.Models.NoSQLDatabaseSchema
{
    public class GrievanceDocument
    {
        public static GrievanceDocumentFields Fields { get; } = new GrievanceDocumentFields();
    }

    public class GrievanceDocumentFields
    {
        public string GuidString { get; } = "guid";
        public string PaginationId { get; } = "pagination_id";
        public string TaxMapId { get; } = "tax_map_id";
        public string Email { get; } = "email";
        public string SubmitDate { get; } = "submit_date";

        public string RequestedPersonalHearing { get; } = "requested_personal_hearing";
        public string CompletedPersonalHearing { get; } = "completed_personal_hearing";

        public string ComplaintType { get; } = "complaint_type";
        public string ProposedValue { get; } = "proposed_value";
        public string CreatorName { get; } = "creator_name";

        public string Downloaded { get; } = "downloaded";
        public string DownloadDate { get; } = "download_date";
        public string DownloadDateUnix { get; } = "download_date_unix";
        public string BarReviewed { get; } = "barReviewed";
        public string BarReviewDate { get; } = "barReviewDate";
        public string BarReviewDateUnix { get; } = "barReviewDateUnix";

        public string DispositionEmailSent { get; } = "email_sent";
        public string DispositionEmailSentDate { get; } = "email_sent_date";

        public string IncludesConflict { get; } = "includes_conflict";
        public string IncludesResQuestionnaire { get; } = "includes_res_questionnaire";
        public string IncludesComQuestionnaire { get; } = "includes_com_questionnaire";
        public string IncludesLetterOfAuth { get; } = "includes_letter_of_auth";
        public string IncludesIncomeExpenseForms { get; } = "includes_income_expense_forms";
        public string IncludesIncomeExpenseExclusion { get; } = "includes_income_expense_exclusion";
        public string IncludesSupportingDocumentation { get; } = "includes_supporting_docs";
        public string FiveSignatureType { get; } = "five_signature_type";

        public string Complainant { get; } = "complainant";
        public string AttorneyGroup { get; } = "attorney_group";
        public string AttorneyEmail { get; } = "attorney_email";
        public string AttorneyPhone { get; } = "attorney_phone";
        public string AttorneyDataRaw { get; } = "attorney_data_raw";
        public string ComplainantMailAddress { get; } = "complainant_mail_address";
        public string CoOpUnitNum { get; } = "co_op_unit_num";
        public string Reason { get; } = "reason";
        public string Notes { get; } = "notes";

        public string NysRP525Tentative { get; } = "nys_rp525_tentative";
        public string NysRP525IsReduced { get; } = "nys_rp525_is_reduced";
        public string NysRP525IsReducedValue { get; } = "nys_rp525_is_reduced_value";
        public string NysRP525IsNotReduced { get; } = "nys_rp525_is_not_reduced";
        public string NysRP525Answers { get; } = "nys_rp525_answers";

        public string IsDeleted { get; } = "IsDeleted";
        public string InternalDatabaseId { get; } = "_id";
    }
}
