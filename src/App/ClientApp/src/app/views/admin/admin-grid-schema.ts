export const AdminGridColumnDefinitions: Array<{headerName: string, field: string, checkboxSelection?: boolean}> = [
    { headerName: 'Tax Map ID', field: 'tax_map_id', checkboxSelection: true },
    { headerName: 'Grievance ID', field: 'guid' },
    { headerName: 'Complainant', field: 'complainant' },
    { headerName: 'Submitter Email', field: 'email' },
    { headerName: 'Attorney Email', field: 'attorney_email' },
    { headerName: 'Submit Date', field: 'submit_date' },
    { headerName: 'Attorney/Rep Group', field: 'attorney_group' },
    { headerName: 'Attorney/Rep Phone', field: 'attorney_phone' },
    { headerName: 'Complainant Mailing Address', field: 'complainant_mail_address' },
    { headerName: 'Co-Op Unit Num', field: 'co_op_unit_num' },
    { headerName: 'Reason', field: 'reason' },
    { headerName: 'Notes', field: 'notes' },

    { headerName: 'Hearing Requested', field: 'requested_personal_hearing' },
    { headerName: 'Hearing Completed', field: 'completed_personal_hearing' },
    { headerName: 'Complaint Type', field: 'complaint_type' },
    { headerName: 'Proposed Value', field: 'proposed_value' },
    { headerName: 'Created By', field: 'creator_name' },

    { headerName: 'Bar Review Status', field: 'barReviewed' },
    { headerName: 'Bar Review Date', field: 'barReviewDate' },
    { headerName: 'Downloaded', field: 'downloaded' },
    { headerName: 'Download Date', field: 'download_date' },

    { headerName: 'Conflict of Interest', field: 'includes_conflict' },
    { headerName: 'Res Questionnaire', field: 'includes_res_questionnaire' },
    { headerName: 'Com Questionnaire', field: 'includes_com_questionnaire' },
    { headerName: 'Includes Auth Letter', field: 'includes_letter_of_auth' },

    { headerName: 'Includes Income Expense Forms', field: 'includes_income_expense_forms' },
    { headerName: 'Includes Income Expense Exclusion', field: 'includes_income_expense_exclusion' },
    { headerName: 'Includes Supporting Docs', field: 'includes_supporting_docs' },

    { headerName: 'Signature Type', field: 'five_signature_type' },

    { headerName: 'RP-525 Tentative', field: 'nys_rp525_tentative' },
    { headerName: 'RP-525 Is Reduced', field: 'nys_rp525_is_reduced' },
    { headerName: 'RP-525 Reduced To', field: 'nys_rp525_is_reduced_value' },
    { headerName: 'RP-525 All Answers', field: 'nys_rp525_answers' }
];

export const AdminGridDefaultColumnDef = {
    sortable: true,
    filter: true,
    resizable: true,
    enableCellChangeFlash: true
};
