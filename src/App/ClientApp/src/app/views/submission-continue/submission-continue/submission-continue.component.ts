import { Component, OnInit } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';
import { Router } from '@angular/router';
import { TimelineValidationService } from 'src/app/services/timeline.service';
import { BrowserSnifferService } from 'src/app/services/browser-sniffer.service';

@Component({
    selector: 'app-submission-continue',
    templateUrl: './submission-continue.component.html',
    styleUrls: ['./submission-continue.component.css']
})
export class SubmissionContinueComponent implements OnInit {

    public IsValidatingGuid: boolean = false;
    public IsUploading: boolean = false;
    public IncludesPersonalHearing: boolean = false;
    public IncludesConflictOfInterest: boolean = false;
    public IncludesResQuestionnaire: boolean = false;
    public IncludesComQuestionnaire: boolean = false;
    public IncludesLetterOfAuthorization: boolean = false;

    public IncludesIncomeExpenseForms: boolean = false;
    public IncludesIncomeExpenseExclusion: boolean = false;
    public IncludesSupportingDocumentation: boolean = false;

    public TaxMapId: string = '';
    public Guid: string = '';
    public GuidIsValid: boolean = false;

    constructor(
        private readonly httpService: HttpService,
        private readonly router: Router,
        private readonly timeline: TimelineValidationService,
        private readonly browserSniffer: BrowserSnifferService
    ) {
        //
    }

    public ngOnInit(): void {
        if (this.browserSniffer.TestBrowserValidity() === false) {
            // Disable the router guard
            // this.formData.UserWantsToSubmit = true;
            this.router.navigate(['/warning']);
            return;
        }
    }

    public ValidateGuid(alertFunc = window.alert) {
        if (this.timeline.TestSupportingDocsUploadDateValidity() === false) {
            alertFunc('This application is closed to further submissions.');
            return;
        }
        if (this.Guid?.length === 0) {
            return alertFunc('Please enter a submission ID.');
        }
        this.IsValidatingGuid = true;
        this.httpService.ValidateGuid(this.Guid.trim())
            .subscribe(
                (result) => {
                    this.GuidIsValid = result.isValid;
                    this.TaxMapId = result.taxMapId;
                    if (this.GuidIsValid === false) {
                        window.alert(
                            'The ID you entered was invalid. \n\n' +
                            'You\'ll need a valid ID to continue your submission. \n\n' +
                            'If you made your initial submission in the last 5 minutes, ' +
                            'please wait 5 minutes for your submission to process.'
                        );
                    }
                },
                (error) => {
                    window.alert('An error occurred while validating your reference ID.');
                    console.error(error);
                },
                () => {
                    this.IsValidatingGuid = false;
                }
            );
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

    // TODO: Refactor to abstraction shared with submit-component
    private validateUploadInputs(formElement: any): boolean {
        const expectedFileCount = this.countExpectedFiles();
        const actualFileCount = formElement.elements.namedItem('files').files.length;

        if (actualFileCount === 0) {
            window.alert(
                'Submission not uploaded. \n' +
                'Please select at least one file to upload.'
            );
            return false;
        }

        if (actualFileCount < expectedFileCount) {
            window.alert(
                'Submission not uploaded. \n\n' +
                `You\'ve indicated via the checkboxes that you\'re attaching at least ${expectedFileCount} files,` +
                `but you\'ve only included ${actualFileCount} file(s). \n\n` +
                'Please attach more files, or uncheck the necessary boxes.'
            );
            return false;
        }

        if (
            // @ts-ignore
            formElement.elements.namedItem('inputEmail').value.length === 0
            ||
            // @ts-ignore
            formElement.elements.namedItem('inputEmailConfirm').value.length === 0
            ||
            // @ts-ignore
            (formElement.elements.namedItem('inputEmail').value.toLowerCase()
                !==
                formElement.elements.namedItem('inputEmailConfirm').value.toLowerCase())
            ||
            // @ts-ignore
            formElement.elements.namedItem('inputEmail').value.includes('@') === false
        ) {
            window.alert(
                'Files not uploaded. \n' +
                'Please ensure the email addresses you entered are valid and match each other.'
            );
            return false;
        }
        return true;
    }

    // TODO: Refactor to abstraction shared with submit-component
    public async UploadFiles(id: string): Promise<void> {
        const formElement = document.getElementById(id);
        if (this.validateUploadInputs(formElement) === false) {
            return;
        }
        this.IsUploading = true;
        // @ts-ignore
        const formData = new FormData(formElement);
        // for (const pair of formData.entries()) {
        //     console.log(pair[0] + ', ' + pair[1]);
        // }
        try {
            // TODO: Refactor this to the HttpService
            const response = await fetch(
                '/api/upload/PostAppendSupportingDocs',
                {
                    method: 'POST',
                    body: formData
                }
            );
            if (response.ok) {
                window.alert(
                    'File(s) submitted successfully. You should receive a confirmation email at address you specified. \n\n' +
                    'If it appears you did not recieve a confirmation email, check your spam/junk folder.'
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
            window.alert(
                'An error occurred. Please retry. \n' +
                'If this issue persists, please submit a support request.'
            );
        }
        finally {
            this.IsUploading = false;
        }
    }
}
