export interface IAssessmentGrievance {
    guid: string;
    tax_map_id: string;
    email: string;
    submit_date: string;
    requested_personal_hearing: boolean;
    completed_personal_hearing: boolean;
    complaint_type: string;
    proposed_value: string;
    creator_name: string;
    downloaded: boolean;
    download_date: string;

    barReviewed: boolean;
    barReviewDate: string;

    includes_conflict: boolean;
    includes_res_questionnaire: boolean;
    includes_com_questionnaire: boolean;
    includes_letter_of_auth: boolean;
    includes_income_expense_forms: boolean;
    includes_income_expense_exclusion: boolean;
    includes_supporting_docs: boolean;

    nys_rp525_tentative: string;
    nys_rp525_is_reduced: string;
    nys_rp525_is_reduced_value: string;
    nys_rp525_answers: string;

    complainant: string;
    attorney_group: string;
    attorney_phone: string;
    complainant_mail_address: string;
    co_op_unit_num: string;
    reason: string;
    notes: string;
}
