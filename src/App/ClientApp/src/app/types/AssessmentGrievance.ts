// tslint:disable: variable-name
import { IAssessmentGrievance } from './IAssessmentGrievance';

export class AssessmentGrievance implements IAssessmentGrievance {

    public guid: string = '';

    public tax_map_id: string = '';
    public email: string = '';
    public submit_date: string = '';
    public requested_personal_hearing: boolean = false;
    public completed_personal_hearing: boolean = false;
    public complaint_type: string = '';
    public proposed_value: string = '';
    public creator_name: string = '';
    public downloaded: boolean = false;
    public download_date: string = '';

    public barReviewed: boolean = false;
    public barReviewDate: string = '';

    public includes_conflict: boolean = false;
    public includes_res_questionnaire: boolean = false;
    public includes_com_questionnaire: boolean = false;
    public includes_letter_of_auth: boolean = false;
    public includes_income_expense_forms: boolean = false;
    public includes_income_expense_exclusion: boolean = false;
    public includes_supporting_docs: boolean = false;

    public nys_rp525_tentative: string = '';
    public nys_rp525_is_reduced: string = '';
    public nys_rp525_is_reduced_value: string = '';
    public nys_rp525_answers: string = '';

    public complainant: string = '';
    public attorney_group: string = '';
    public attorney_phone: string = '';
    public complainant_mail_address: string = '';
    public co_op_unit_num: string = '';
    public reason: string = '';
    public notes: string = '';

    constructor() {
        //
    }
}
