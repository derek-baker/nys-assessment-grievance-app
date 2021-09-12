import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { ClientStorageService } from 'src/app/services/client-storage.service';
import 'bootstrap';
// @ts-ignore
import $ from 'jquery';
import { Router } from '@angular/router';
import { FormDataService } from 'src/app/services/form-data.service';
import { IAttorneyPrefillData } from 'src/app/types/IAttorneyPrefillData';
import { TimelineValidationService } from 'src/app/services/timeline.service';
import { BrowserSnifferService } from 'src/app/services/browser-sniffer.service';
import { HttpPublicService } from 'src/app/services/http.service.public';

@Component({
    selector: 'app-form-controls',
    templateUrl: './form-controls.component.html',
    styleUrls: ['./form-controls.component.css']
})
export class FormControlsComponent implements OnInit {

    @Input()
    public readonly ParentForm: FormGroup;

    public IsFetchingData: boolean = false;
    public Attorneys: Array<IAttorneyPrefillData> = [];
    public AttorneyForm = new FormGroup(
        // Set form's initial value to 'Pro-Se'
        { AttorneyControl: new FormControl( ) } // this.Attorneys[0] ) }
    );
    public TermsAccepted: boolean = false;

    private readonly termsModalId: string = 'AcceptTermsModal';
    private readonly proSeLabel = 'Pro Se | Individuals';

    constructor(
        private readonly storage: ClientStorageService,
        private readonly router: Router,
        private readonly formData: FormDataService,
        private readonly browserSniffer: BrowserSnifferService,
        private readonly timeline: TimelineValidationService,
        private readonly httpPublic: HttpPublicService
    ) {
        this.TermsAccepted = this.storage.GetTermsAccepted();
    }

    public ngOnInit(): void {
        // TODO: Refactor
        if (this.browserSniffer.TestBrowserValidity() === false) {
            // Disable the router guard
            this.formData.UserWantsToSubmit = true;
            this.router.navigate(['/warning']);
            return;
        }

        this.checkForSmallWidthDevice();

        this.Attorneys = [
            {
                GroupNo:  '100',
                GroupName1:  this.proSeLabel,
                ContactName: undefined,
                Address1: undefined,
                Address2: undefined,
                City: undefined,
                State: undefined,
                ZipCode: undefined,
                Phone1: undefined,
                Phone2: undefined,
                FAX1: undefined,
                FAX2: undefined,
                Email: undefined
            }
        ];

        this.IsFetchingData = true;
        this.httpPublic.GetRepresentatives().subscribe(
            (data: Array<IAttorneyPrefillData>) => {
                this.IsFetchingData = false;
                this.Attorneys.push(...data);
            },
            (error) => {
                window.alert('An error occurred.');
                console.error(error);
                this.IsFetchingData = false;
            }
        );

        if (this.storage.GetTermsAccepted() === false) {
            this.toggleTermsModal();
        }
    }

    public acceptTermsEventHandler(): void {
        this.TermsAccepted = true;
        this.storage.SetTermsAccepted();
        this.toggleTermsModal();
    }

    private toggleTermsModal(
        modalId: string = this.termsModalId
    ) {
        $(`#${modalId}`).modal('toggle');
    }
    private showTermsModal(
        modalId: string = this.termsModalId
    ) {
        $(`#${modalId}`).modal('show');
    }

    /**
     * These can be attorneys or reps that perform these submissions.
     */
    public FillAttorneyData(
        attorneyControl = this.AttorneyForm.value.AttorneyControl,
        attorneyGroup = attorneyControl.GroupName1
    ) {
        if (attorneyGroup !== this.proSeLabel) {
            const repEmail = attorneyControl.Email;

            // const attorneyGroup = attorneyControl.GroupName1;
            // name address phone email
            const repInfo =
                `Group: ${attorneyControl.GroupName1 ?? 'unknown'}, ` +
                `Group-No: ${attorneyControl.GroupNo ?? 'unknown'}, ` +
                `Contact: ${attorneyControl.ContactName ?? 'unknown'}, ` +
                `Phone: ${attorneyControl.Phone1 ?? 'unknown'}, ` +
                `Email: ${repEmail ?? 'unknown'}`;

            this.ParentForm.controls.RepInfo.setValue(repInfo);
            this.ParentForm.controls.RepInfoComplete.setValue(JSON.stringify(attorneyControl));
            this.ParentForm.controls.four_2_text.setValue(attorneyGroup);
            this.ParentForm.controls.four_signature_name.setValue(attorneyGroup);
            this.ParentForm.controls.five_signature_name.setValue(attorneyGroup);
            this.ParentForm.controls.six_signature_name.setValue(attorneyGroup);

            // this.ParentForm.controls.OwnerAddressLine1.setValue(
            //     // `Represented by: ${attorneyGroup}`
            //     ''
            // );
            // const address2Text =
            //     (attorneyControl.Address1 || attorneyControl.Address1.length > 0)
            //         ? attorneyControl.Address1 : '';
            // this.ParentForm.controls.OwnerAddressLine1.setValue(
            //     `${attorneyControl.Address1} ${address2Text}`
            // );

            // this.ParentForm.controls.OwnerAddressLine2.setValue(
            //     `${attorneyControl.City} ${attorneyControl.State}, ${attorneyControl.ZipCode}`
            // );
            this.ParentForm.controls.Email.setValue(repEmail);
        }
    }

    /**
     * Portait layout on phones doesn't lay out ideally, so we do this.
     */
    private checkForSmallWidthDevice(
        innerWidth: number = window.innerWidth,
        inPortraitOrientation: boolean = window.matchMedia('(orientation: portrait)').matches
    ): boolean {
        if (inPortraitOrientation === true && innerWidth < 1000) {
            window.alert(
                'It appears your viewport is in a portrait orientation on a small device. \n' +
                'If you rotate your device into a landscape orientation, or otherwise increase your screen width,' +
                'you may find the layout of the form more preferrable.'
            );
            return false;
        }
        return true;
    }

    // TODO: Handle user zoom

    public ClearForm() {
        this.ParentForm.reset();
    }

    public DisplayTerms() {
        this.TermsAccepted = false;
        this.showTermsModal();
    }

    private validateForm(
        errorMsgFirstLine = 'FORM IS INCOMPLETE OR INVALID. \n'
    ): boolean {
        if (this.TermsAccepted === false) {
            window.alert('Please accept the terms and conditions.');
            this.showTermsModal();
        }
        const dayPhone: string = this.ParentForm.controls.DayPhone.value;
        if (
            dayPhone
            &&
            dayPhone.length > 0
            &&
            dayPhone.includes('-') === false
        ) {
            window.alert(
                errorMsgFirstLine +
                'Please ensure the day phone number you entered in 1.1 is formatted with dashes (EX: XXX-XXX-XXXX).'
            );
            return false;
        }

        const eveningPhone: string = this.ParentForm.controls.EveningPhone.value;
        if (
            eveningPhone
            &&
            eveningPhone.length > 0
            &&
            eveningPhone.includes('-') === false
        ) {
            window.alert(
                errorMsgFirstLine +
                'Please ensure the evening phone number you entered in 1.1 is formatted with dashes (EX: XXX-XXX-XXXX).'
            );
            return false;
        }

        // Validate email 1.2
        const email: string = this.ParentForm.controls.Email.value;
        if (
            email?.length === 0
            ||
            email?.includes('@') === false
            ||
            email?.includes('.') === false
        ) {
            window.alert(
                errorMsgFirstLine +
                'Please ensure the email address you entered in 1.2 is valid.'
            );
            return false;
        }

        // Validate tax map id 1.5
        const taxMapNum = this.ParentForm.controls.TaxMapNum.value;
        if (!taxMapNum || taxMapNum.length === 0) {
            window.alert(
                errorMsgFirstLine +
                'You must enter a tax map ID in 1.5.'
            );
            return false;
        }

        // Validate 1.7
        const estimate =
            (this.ParentForm.controls.MarketValueEstimate.value)
                ? this.ParentForm.controls.MarketValueEstimate.value.length
                : 0;
        if (estimate === 0) {
            window.alert(
                errorMsgFirstLine +
                'You must enter an estimate in 1.7.'
            );
            return false;
        }

        // Validate part 5 signature
        if (
            this.ParentForm.controls.five_signature_name.value.length === 0
            ||
            document.getElementById('signature2').getAttribute('src') === ''
        ) {
            window.alert(
                errorMsgFirstLine +
                'If there is no one representing you, indicate "NONE" or "N/A" in the Part 5 designate box.'
            );
            return false;
        }
        return true;
    }

    public async BeginSubmission() {
        if (this.timeline.TestInitialSubmissionDateValidity() === false) { return; }
        if (this.validateForm() === false) { return; }

        const msg =
            'Do you want to continue to the next section? \n' +
            'You will not be able to return to this page. \n' +
            'Click \'OK\' to continue to the next section.';
        if (window.confirm(msg) === false) { return; }

        // TODO: A method to handle capturing appropriate data
        this.formData.UserWantsToSubmit = true;
        this.formData.TaxMapId = this.formData.GetFormValue('TaxMapNum');
        this.formData.Email = this.formData.GetFormValue('Email');
        this.formData.FormData = this.formData.ExtractFormValues();

        this.router.navigate(['/submit']);
    }
}
