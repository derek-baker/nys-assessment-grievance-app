import { Component, OnInit, Input, Output, EventEmitter, AfterViewChecked } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';
import { FormControl, FormGroup } from '@angular/forms';
import { GrievanceReasons } from 'src/app/types/GrievanceReasons';
import { IAttorneyPrefillData } from 'src/app/types/IAttorneyPrefillData';
import { HttpPublicService } from 'src/app/services/http.service.public';

@Component({
    selector: 'app-modal-create-submission',
    templateUrl: './modal-create-submission.component.html',
    styleUrls: ['./modal-create-submission.component.css']
})
export class ModalCreateSubmissionComponent implements OnInit, AfterViewChecked {

    @Input()
    public UserName: string;

    /** Should be used to emit an event that will trigger desired behavior */
    @Output()
    private reloadGridEvent = new EventEmitter<string>();

    public Attorneys: Array<IAttorneyPrefillData> = [];

    public ApplicantEmail: string = '';
    public ApplicantEmailConfirm: string = '';
    public TaxMapId: string = '';
    public ProposedValue: string = '';

    public Complainant: string = '';
    public AttorneyDataRaw: string = '';
    public AttorneyGroup: string = '';
    public AttorneyPhone: string = '';
    public ComplainantMailAddress: string = '';
    public CoOpUnitNum: string = '';
    public Notes: string = '';

    public IsUploading: boolean = false;
    public IncludesPersonalHearing: boolean = false;
    public IncludesConflictOfInterest: boolean = false;
    public IncludesResQuestionnaire: boolean = false;
    public IncludesComQuestionnaire: boolean = false;
    public IncludesLetterOfAuthorization: boolean = false;

    public IncludesIncomeExpenseForms: boolean = false;
    public IncludesIncomeExpenseExclusion: boolean = false;
    public IncludesSupportingDocumentation: boolean = false;

    public readonly ComplaintTypes: Array<string> = [
        'A. UNEQUAL ASSESSMENT',
        'B. EXCESSIVE ASSESSMENT',
        'C. UNLAWFUL ASSESSMENT',
        'D. MISCLASSIFICATION'
    ];
    public GrievanceReasons = GrievanceReasons;

    public readonly ComplaintTypeSelectControl: FormControl = new FormControl();
    public readonly ReasonSelectControl: FormControl = new FormControl();
    public readonly RepGroupSelectControl: FormControl = new FormControl();

    constructor(
        private readonly http: HttpService,
        private readonly httpPublic: HttpPublicService
    ) {}

    public ngOnInit(): void {
        this.Attorneys = [
           {
               GroupNo:  '100',
               GroupName1:  'Pro Se - Individuals',
               ContactName: null,
               Address1: null,
               Address2: null,
               City: null,
               State: null,
               ZipCode: null,
               Phone1: null,
               Phone2: null,
               FAX1: null,
               FAX2: null,
               Email: null
           }
        ];
        this.httpPublic.GetRepresentatives().subscribe(
            (data: Array<IAttorneyPrefillData>) => {
                this.Attorneys.push(...data);
            },
            (error) => {
                window.alert('An error occurred.');
                console.error(error);
            }
        );
    }

    public ngAfterViewChecked(): void {
        this.ComplaintTypeSelectControl.setValue(this.ComplaintTypes[1]);
    }

    private resetForm() {
        this.ApplicantEmail = '';
        this.ApplicantEmailConfirm = '';

        this.TaxMapId = '';
        this.ProposedValue = '';
        this.Complainant = '';
        this.AttorneyGroup = '';
        this.AttorneyDataRaw = '';
        this.AttorneyPhone = '';
        this.ComplainantMailAddress = '';
        this.CoOpUnitNum = '';
        this.Notes = '';

        this.ComplaintTypeSelectControl.reset();
        this.ReasonSelectControl.reset();
        this.RepGroupSelectControl.reset();

        this.IncludesPersonalHearing = false;
        this.IncludesConflictOfInterest = false;
        this.IncludesResQuestionnaire = false;
        this.IncludesComQuestionnaire = false;
        this.IncludesLetterOfAuthorization = false;

        this.ComplaintTypeSelectControl.setValue(this.ComplaintTypes[1]);
        (document.getElementById('files_submission_creation') as HTMLInputElement).value = '';
    }

    public DownloadFillableNysRp524() {
        this.http.GetNysRp524WithFillableBoardOnly()
            .subscribe(
                (result) => {
                    // TODO: Refactor to service?
                    const blob = new Blob(
                        [result],
                        {
                            type: 'application/pdf' // must match the Accept type
                        }
                    );
                    const link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'NYS_RP-524_CompletelyFillable.pdf';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                    link.remove();
                },
                (err) => {
                    console.error(err);
                    window.alert('An error occurred. Please retry.');
                }
            );
    }

    public FillAttorneyData(
        attorneyGroup: IAttorneyPrefillData = this.RepGroupSelectControl.value
    ): void {
        this.AttorneyDataRaw = JSON.stringify(attorneyGroup);
        this.AttorneyGroup = attorneyGroup.GroupName1;
        this.AttorneyPhone = attorneyGroup.Phone1;
        this.ApplicantEmail = attorneyGroup.Email;
        this.ApplicantEmailConfirm = attorneyGroup.Email;
        this.ComplainantMailAddress =
            (attorneyGroup.Address1 ?? '') + ' ' +
            (attorneyGroup.Address2 ?? '') + ' ' +
            (attorneyGroup.City ?? '') + ' ' +
            (attorneyGroup.State ?? '') + ' ' +
            (attorneyGroup.ZipCode ?? '');
    }

    public HandleComplaintTypeSelectChange() {
        // this.SetSelectedRepGroups = this.RepGroupSelectControl.value;
    }
    public HandleReasonSelectChange() {
        //
    }

    private validateForm(formElement: any): boolean {
        // const expectedFileCount = this.countExpectedFiles();
        const actualFileCount = formElement.elements.namedItem('files').files.length;

        if (actualFileCount === 0) {
            window.alert(
                'Submission not uploaded. \n' +
                'Please select at least one file to upload.'
            );
            return false;
        }
        if (
            this.ApplicantEmail?.length === 0
            ||
            this.ApplicantEmailConfirm.length === 0
            ||
            (this.ApplicantEmail.toLowerCase() !== this.ApplicantEmailConfirm.toLowerCase())
            ||
            this.ApplicantEmail.includes('@') === false
            ||
            this.ApplicantEmail.includes('.') === false
            ||
            this.TaxMapId.length === 0
        ) {
            window.alert(
                'Grievance not created. \n\n' +
                'Please ensure the email addresses you entered are valid and match each other, ' +
                'and that you included the Tax Map ID for the parcel in question.'
            );
            return false;
        }

        return true;
    }

    /**
     * TODO: Refactor to HTTP Service
     * This should never enforce submission dates (in case you were wondering)
     */
    public async UploadSubmission(
        formId: string = 'CreateSubmissionForm',
        // TODO: Refactor to config
        endpoint = 'api/admin/PostCreateSubmission'
    ) {
        const formElement = document.getElementById(formId);
        if (this.validateForm(formElement) === false) {
            return;
        }
        this.IsUploading = true;
        // @ts-ignore
        const formData = new FormData(formElement);
        // for (const pair of formData.entries()) {
        //     console.log(pair[0] + ', ' + pair[1]);
        // }
        try {
            const response = await fetch(
                endpoint,
                {
                    method: 'POST',
                    body: formData
                }
            );
            if (response.ok) {
                this.reloadGridEvent.emit();
                window.alert(
                    'Grievance created successfully. ' +
                    'You can close this alert and the modal under it.'
                );
                this.resetForm();
            }
            else {
                console.error(response);
                window.alert('ERROR: Please retry');
            }
        }
        catch (error) {
            console.error(error);
            window.alert('ERROR: Please retry');
        }
        finally {
            this.IsUploading = false;
        }
    }
}
