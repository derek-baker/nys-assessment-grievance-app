namespace Library.Models
{
    /// <summary>
    /// In-app, user-facing model of grievance
    /// </summary>
    public class GrievanceApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "<Pending>")]
        public string guid { get; set; }
        public string tax_map_id { get; set; }
        public string email { get; set; }
        public string submit_date { get; set; }
        public bool requested_personal_hearing { get; set; }
        public bool completed_personal_hearing { get; set; }
        public string complaint_type { get; set; }
        public string proposed_value { get; set; }
        public string creator_name { get; set; }
        public bool downloaded { get; set; }
        public string download_date { get; set; }
        public bool barReviewed { get; set; }
        public string barReviewDate { get; set; }
        public bool email_sent { get; set; }
        public string email_sent_date { get; set; }

        public bool includes_conflict { get; set; }
        public bool includes_res_questionnaire { get; set; }
        public bool includes_com_questionnaire { get; set; }
        public bool includes_letter_of_auth { get; set; }
        public bool includes_income_expense_forms { get; set; }
        public bool includes_income_expense_exclusion { get; set; }
        public bool includes_supporting_docs { get; set; }
        public string five_signature_type { get; set; }

        public string nys_rp525_tentative { get; set; }
        public string nys_rp525_is_reduced { get; set; }
        public string nys_rp525_is_reduced_value { get; set; }
        public string nys_rp525_answers { get; set; }        

        public string complainant { get; set; }
        public string attorney_group { get; set; }
        public string attorney_email { get; set; }
        public string attorney_phone { get; set; }
        public string attorney_data_raw { get; set; }
        public string complainant_mail_address { get; set; }
        public string co_op_unit_num { get; set; }
        public string reason { get; set; }
        public string notes { get; set; }

    }
}
