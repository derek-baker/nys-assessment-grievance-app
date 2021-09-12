namespace Library.Models.NoSQLDatabaseSchema
{
    public static class GrievanceDocumentFields
    {
        public static string GuidString { get; } = "guid";
        public static string PaginationId { get; } = "pagination_id";
        public static string TaxMapId { get; } = "tax_map_id";
        public static string Email { get; } = "email";
        public static string SubmitDate { get; } = "submit_date";

        public static string RequestedPersonalHearing { get; } = "requested_personal_hearing";
        public static string CompletedPersonalHearing { get; } = "completed_personal_hearing";

        public static string ComplaintType { get; } = "complaint_type";
        public static string ProposedValue { get; } = "proposed_value";
        public static string CreatorName { get; } = "creator_name";

        public static string Downloaded { get; } = "downloaded";
        public static string DownloadDate { get; } = "download_date";
        public static string DownloadDateUnix { get; } = "download_date_unix";
        public static string BarReviewed { get; } = "barReviewed";
        public static string BarReviewDate { get; } = "barReviewDate";
        public static string BarReviewDateUnix { get; } = "barReviewDateUnix";

        public static string DispositionEmailSent { get; } = "email_sent";
        public static string DispositionEmailSentDate { get; } = "email_sent_date";

        public static string IncludesConflict { get; } = "includes_conflict";
        public static string IncludesResQuestionnaire { get; } = "includes_res_questionnaire";
        public static string IncludesComQuestionnaire { get; } = "includes_com_questionnaire";
        public static string IncludesLetterOfAuth { get; } = "includes_letter_of_auth";
        public static string IncludesIncomeExpenseForms { get; } = "includes_income_expense_forms";
        public static string IncludesIncomeExpenseExclusion { get; } = "includes_income_expense_exclusion";
        public static string IncludesSupportingDocumentation { get; } = "includes_supporting_docs";
        public static string FiveSignatureType { get; } = "five_signature_type";

        public static string Complainant { get; } = "complainant";
        public static string AttorneyGroup { get; } = "attorney_group";
        public static string AttorneyEmail { get; } = "attorney_email";
        public static string AttorneyPhone { get; } = "attorney_phone";
        public static string AttorneyDataRaw { get; } = "attorney_data_raw";
        public static string ComplainantMailAddress { get; } = "complainant_mail_address";
        public static string CoOpUnitNum { get; } = "co_op_unit_num";
        public static string Reason { get; } = "reason";
        public static string Notes { get; } = "notes";

        public static string NysRP525Tentative { get; } = "nys_rp525_tentative";
        public static string NysRP525IsReduced { get; } = "nys_rp525_is_reduced";
        public static string NysRP525IsReducedValue { get; } = "nys_rp525_is_reduced_value";
        public static string NysRP525IsNotReduced { get; } = "nys_rp525_is_not_reduced";
        public static string NysRP525Answers { get; } = "nys_rp525_answers";

        public static string IsDeleted { get; } = "IsDeleted";
        public static string InternalDatabaseId { get; } = "_id";
    }
}
