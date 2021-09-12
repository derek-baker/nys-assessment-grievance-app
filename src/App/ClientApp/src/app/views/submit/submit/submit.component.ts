import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormDataService } from 'src/app/services/form-data.service';
import { HttpService } from 'src/app/services/http.service';
import { BrowserSnifferService } from 'src/app/services/browser-sniffer.service';
import { GrievanceReasons } from 'src/app/types/GrievanceReasons';
import { FormControl } from '@angular/forms';

@Component({
    selector: 'app-submit',
    templateUrl: './submit.component.html',
    styleUrls: ['./submit.component.css']
})
export class SubmitComponent implements OnInit {

    private reCapchaChecked: boolean = false;

    public readonly Attachments: Array<any> = [];
    public TaxMapId: string;
    public Email: string;

    public IsUploading: boolean = false;
    public IncludesPersonalHearing: boolean = false;
    public IncludesConflictOfInterest: boolean = false;
    public IncludesResQuestionnaire: boolean = false;
    public IncludesComQuestionnaire: boolean = false;
    public IncludesLetterOfAuthorization: boolean = false;

    public IncludesIncomeExpenseForms: boolean = false;
    public IncludesIncomeExpenseExclusion: boolean = false;
    public IncludesSupportingDocumentation: boolean = false;

    public SerializedFormData: string;
    public Base64SignatureFromPartFour: string;
    public Base64SignatureFromPartFive: string;

    public readonly GrievanceReasons = GrievanceReasons;
    public readonly ReasonSelectControl: FormControl = new FormControl();

    constructor(
        private readonly router: Router,
        private readonly formDataFrom524: FormDataService,
        private readonly httpService: HttpService,
        private readonly browserSniffer: BrowserSnifferService
    ) {
        // @ts-ignore
        window.ReCapchaCallbackOne = this.reCapchaCallback.bind(this);
    }

    public ngOnInit(): void {
        if (this.browserSniffer.TestBrowserValidity() === false) {
            this.router.navigate(['/warning']);
            return;
        }

        window.scrollTo({top: 0});

        // INTENT: If you click away from this view/component, the RECAPCHA won't re-render without the code below.
        const capcha = document.querySelector('.g-recaptcha');

        // TODO: Refactor site key to assets/config.json
        // @ts-ignore
        grecaptcha.render(capcha, { sitekey: '6Lc-KvoUAAAAAJESyR-eA3xXzkimmniZL6VfJHfS' });
        this.TaxMapId = this.formDataFrom524.TaxMapId;
        this.Email = this.formDataFrom524.Email;

        // If this condition returns false, it indicates that something is wrong.
        if (this.TaxMapId?.length === 0) {
            window.alert(
                'Please fill out the online RP-524 before attempting to submit'
            );
            this.router.navigate(['/rp524']);
            return;
        }
        this.httpService.CheckForPreviousSubmissionsForParcel(
            this.Email,
            this.TaxMapId
        ).subscribe(
            (result: {message: string}) => {
                if (result.message && result.message.length > 0) {
                    window.alert(
                        `It appears that the following individuals have already submitted a RP-524 for ${this.TaxMapId}: \n\n` +
                        `${result.message} \n\n` +
                        'If one of the previous submitters above was you, and your intent is to add supporting documentation ' +
                        'to one of those previously submitted applications, you must use the \'Add Supporting Documentation\' link above.'
                    );
                }
            },
            (error) => {
                console.error(error);
            }
        );
        this.SerializedFormData = JSON.stringify(this.formDataFrom524.FormData);
        this.Base64SignatureFromPartFour = this.formDataFrom524.SignatureFromPart4;
        this.Base64SignatureFromPartFive = this.formDataFrom524.SignatureFromPart5;
    }

    private countExpectedFiles() {
        let numExpectedFiles = 0;
        numExpectedFiles += (this.IncludesPersonalHearing) ? 1 : 0;
        numExpectedFiles += (this.IncludesConflictOfInterest) ? 1 : 0;
        numExpectedFiles += (this.IncludesResQuestionnaire) ? 1 : 0;
        numExpectedFiles += (this.IncludesComQuestionnaire) ? 1 : 0;
        numExpectedFiles += (this.IncludesLetterOfAuthorization) ? 1 : 0;
        numExpectedFiles += (this.IncludesIncomeExpenseForms) ? 1 : 0;
        numExpectedFiles += (this.IncludesIncomeExpenseExclusion) ? 1 : 0;
        numExpectedFiles += (this.IncludesSupportingDocumentation) ? 1 : 0;
        return numExpectedFiles;
    }

    private validateUploadInputs(
        formElement: any,
        reCapchaChecked: boolean
    ): boolean {
        const expectedFileCount = this.countExpectedFiles();
        const actualFileCount = formElement.elements.namedItem('files').files.length;

        if (actualFileCount < expectedFileCount) {
            window.alert(
                'Submission not uploaded. \n' +
                `You\'ve indicated via the checkboxes that you\'re attaching at least ${expectedFileCount} files,` +
                `but you\'ve only included ${actualFileCount} file(s). Please attach more files, or uncheck the necessary boxes.`
            );
            return false;
        }
        // TODO: Bind these to a model instead
        const email: string = formElement.elements.namedItem('input_email').value;
        const emailConfirm: string = formElement.elements.namedItem('input_email_confirm').value;
        if (
            email.length === 0
            ||
            emailConfirm.length === 0
            ||
            (email.toLowerCase() !== emailConfirm.toLowerCase())
            ||
            email.includes('@') === false
            ||
            email.includes('.') === false
            ||
            formElement.elements.namedItem('input_tax_map_id').value.length === 0
            ||
            reCapchaChecked === false
        ) {
            window.alert(
                'Files not uploaded. \n' +
                'Please ensure the email addresses you entered are valid and match each other, ' +
                'and you included the Tax Map ID for the parcel in question, and that you checked ' +
                'the RECAPCHA to the left of the Upload button.'
            );
            return false;
        }
        return true;
    }

    private reCapchaCallback(res: any): void {
        console.log(res);
        this.reCapchaChecked = true;
    }

    public HandleReasonSelectChange() {
        //
    }

    public async UploadInitialSubmissionData(
        id: string
    ) {
        const formElement = document.getElementById(id);
        if (this.validateUploadInputs(formElement, this.reCapchaChecked) === false) {
            return;
        }
        this.IsUploading = true;
        // @ts-ignore
        const formData = new FormData(formElement);
        // // @ts-ignore
        // for (const pair of formData.entries()) {
        //     console.log(pair[0] + ', ' + pair[1]);
        // }
        try {
            // TODO: Refactor this to the HttpService
            const response: Response = await fetch(
                '/api/upload/PostInitialSubmission',
                {
                    method: 'POST',
                    body: formData
                }
            );
            if (response.ok) {
                console.warn(response);
                window.alert(
                    'File(s) submitted successfully. You should receive a confirmation email at address you specified. \n\n' +
                    'If it appears you did not recieve a confirmation email, check your spam/junk folder. \n\n' +
                    'IMPORTANT: If you plan to submit additional documentation later, you\'ll need your submission\'s reference ID.'
                );
                this.router.navigate(['/']);
            }
            else {
                console.error('Reponse was: ');
                console.error(response);
                window.alert(
                    'An error occurred during upload. Please retry.'
                );
            }
        }
        catch (error) {
            console.error('Error:', error);
        }
        finally {
            this.IsUploading = false;
        }
    }
}
