import { Component, OnInit, Input } from '@angular/core';
import { IAssessmentGrievance } from 'src/app/types/IAssessmentGrievance';
import { ISelectedGrievance } from 'src/app/types/ISelectedApplication';
import { AdminComponent } from '../admin.component';

@Component({
    selector: 'app-modal-add-files',
    templateUrl: './modal-add-files.component.html',
    styleUrls: ['./modal-add-files.component.css']
})
export class ModalAddFilesComponent implements OnInit {

    @Input()
    public readonly SelectedApplication: ISelectedGrievance;

    public includesPersonalHearing: boolean;
    public includesConflictOfInterest: boolean;
    public includesResQuestionnaire: boolean;
    public includesComQuestionnaire: boolean;
    public includesLetterOfAuthorization: boolean;

    public IncludesIncomeExpenseForms: boolean = false;
    public IncludesIncomeExpenseExclusion: boolean = false;
    public IncludesSupportingDocumentation: boolean = false;

    public AddingFilesToSubmission: boolean = false;

    constructor(private readonly parent: AdminComponent) { }

    public ngOnInit(): void {
        this.UncheckBoxes();
    }

    public UncheckBoxes() {
        const checkBoxesIds = [
            'AddFiles_IncludesPersonalHearing',
            'AddFiles_IncludesConflictOfInterest',
            'AddFiles_IncludesResQuestionnaire',
            'AddFiles_IncludesComQuestionnaire',
            'AddFiles_IncludesLetterOfAuthorization',
            'AddFiles_IncludesIncomeExpenseForms',
            'AddFiles_IncludesIncomeExpenseExclusion',
            'AddFiles_IncludesSupportingDocumentation'
        ];
        checkBoxesIds.forEach(
            (id: string) => {
                const input = document.getElementById(id);
                if (input) {
                    // @ts-ignore
                    input.checked = false;
                }
            }
        );
    }

    /** TODO: Refactor to use less direct DOM */
    public async AddFileToSubmission(formId: string = 'AddFilesForm') {
        const formElement = document.getElementById(formId);
        if (
            // @ts-ignore
            formElement.elements.namedItem('files').value.length === 0
        ) {
            window.alert(
                'Please select at least one file to upload.'
            );
            return false;
        }
        this.AddingFilesToSubmission = true;
        // @ts-ignore
        const formData = new FormData(formElement);
        // @ts-ignore
        // for (const pair of formData.entries()) {
        //     console.log(pair[0] + ', ' + pair[1]);
        // }

        // TODO: Refactor to HTTP service.
        try {
            const response = await fetch(
                // TODO: Refactor to config
                'api/admin/PostAddDocsToSubmission',
                {
                    method: 'POST',
                    body: formData
                }
            );
            if (response.ok) {
                const dataUpdater = (rowToUpdate: IAssessmentGrievance) => {
                    console.log(rowToUpdate);

                    if (this.includesPersonalHearing === true) { rowToUpdate.requested_personal_hearing = this.includesPersonalHearing; }
                    if (this.includesConflictOfInterest === true) { rowToUpdate.includes_conflict = this.includesConflictOfInterest; }
                    if (this.includesComQuestionnaire === true) { rowToUpdate.includes_com_questionnaire = this.includesComQuestionnaire; }
                    if (this.includesResQuestionnaire === true) { rowToUpdate.includes_res_questionnaire = this.includesResQuestionnaire; }
                    if (this.includesLetterOfAuthorization === true) {
                        rowToUpdate.includes_letter_of_auth = this.includesLetterOfAuthorization; }

                    console.log(rowToUpdate);
                };
                this.parent.refreshGridData(dataUpdater);
                this.UncheckBoxes();

                (document.getElementById('files') as HTMLInputElement).value = '';
                window.alert(
                    'File(s) submitted successfully.'
                );
            }
            else if (response.status === 500) {
                console.error(response);
                window.alert('An error occurred. Please retry.');
            }
            else if (response.status === 403) {
                window.alert('You are not authorized to perform this action.');
            }
        }
        catch (error) {
            console.error('Error:', error);
            window.alert(
                'An error occurred. Please Retry. \n' +
                'If this issue persists, please submit a support request.'
            );
        }
        finally {
            this.AddingFilesToSubmission = false;
        }
    }

}
