import { Component, OnInit, Input } from '@angular/core';
import { AdminComponent } from '../admin.component';
import $ from 'jquery';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SelectedGrievanceService } from 'src/app/services/selected-application.service';
import { FormDataService } from 'src/app/services/form-data.service';
import SignaturePad from 'signature_pad';
import trimCanvas from 'trim-canvas';
import { HttpService } from 'src/app/services/http.service';
import { IRP525PrefillData } from 'src/app/types/IRP525PrefillData';
import { IAssessmentGrievance } from 'src/app/types/IAssessmentGrievance';

interface IExternalService525PrefillData {
    LocationCityTown: string;
    LocationCounty: string;
    LocationStreetAddress: string;
    LocationVillage: string;
    Muni: string;
    OwnerAddressLine1: string;
    OwnerAddressLine2: string;
    OwnerNameLine1: string;
    OwnerNameLine2: string;
    TaxMapNum: string;
    TotalVal: string;
}

@Component({
    selector: 'app-modal-online-bar-review',
    templateUrl: './modal-online-bar-review.component.html',
    styleUrls: ['./modal-online-bar-review.component.css']
})
export class ModalOnlineBarReviewComponent implements OnInit {

    @Input()
    public readonly DownloadFilesAssociatedWithSelectedGrievance: () => void;

    @Input()
    public readonly IsArchiveDownloading: boolean;

    public IsSignaturePadHidden: boolean = true;
    public IsOnlineBarReviewSaving: boolean = false;
    public IsOnlineBarReviewUploading: boolean = false;
    public IsDownloadingGrievanceFiles: boolean = false;
    public IsFetchingPrefillData: boolean = false;

    /**
     * Some FormControls will be added to this group dynamically.
     * This is done in the template by passing a ref to this into components.
     */
    public readonly NysRp525: FormGroup;

    private signatureNameCache: string;
    private signatureDateCache: string;

    private canvas: any;
    private signaturePad: any;
    /** This is actually for a 525, but we would have gotten this data from the 524, or from scraping */
    // @ts-ignore
    private rp524Data: IRP525PrefillData = {};
    private readonly barReviewModalId: string = 'BarReviewOnlineModal';

    constructor(
        private readonly parent: AdminComponent,
        private readonly selectedGrievance: SelectedGrievanceService,
        private readonly formData: FormDataService,
        private readonly http: HttpService
    ) {
        const validators = [Validators.required];
        this.NysRp525 = new FormGroup({
            UserName: new FormControl(''),
            SubmissionGuid: new FormControl('', validators),
            TaxMapId: new FormControl('', validators),
            SubmitterEmail: new FormControl('', validators),
            SignatureAsBase64String: new FormControl('', validators),
            Admin_Rp525_ComplainantInfoTextArea: new FormControl('', validators)
        });

        console.log('TEST')
        console.log(this.parent.UserName)

        // Set up local subscription to value stored in grievance service
        this.selectedGrievance.SelectedGrievance$.subscribe((selected) => {
            // Need to clean up here in case any previous form submission wasn't completed and cleaned
            this.NysRp525.reset();
            // Reload these from memory if possible
            this.NysRp525.controls.Admin_Rp525_SignDate.setValue(this.signatureDateCache);
            this.NysRp525.controls.Admin_Rp525_SignName.setValue(this.signatureNameCache);

            // Take this opportunity to refresh the creds
            this.NysRp525.controls.UserName.setValue(this.parent.UserName);

            // Figure out which submission the user selected before the modal opened
            this.NysRp525.controls.SubmissionGuid.setValue(selected.Guid);
            this.NysRp525.controls.TaxMapId.setValue(selected.TaxMapId);
            this.NysRp525.controls.SubmitterEmail.setValue(selected.Email);

            if (!this.NysRp525.controls.SubmissionGuid.value) {
                console.warn('Skipping fetching of 524 JSON due to lack of submission ID. Please investigate.');
                return;
            }

            this.prefillRp525FromGridData();
            this.http.GetGrievanceJson(
                this.NysRp525.controls.SubmissionGuid.value
            ).subscribe(
                (data: IRP525PrefillData) => {
                    if (data) {
                        this.rp524Data = data;
                        this.prefillRp525();
                    }
                    this.http.GetOnlineBarReview(this.NysRp525.controls.SubmissionGuid.value)
                        .subscribe(
                            (barReviewAnswers) => {
                                if (!barReviewAnswers) { return; }
                                // tslint:disable-next-line: forin
                                for (const propName in barReviewAnswers) {
                                    // TODO: .NET Core camel-cases JSON when serializing...
                                    const propNameWithCapitalFirstLetter = propName.replace(
                                        propName[0],
                                        propName[0].toUpperCase()
                                    );

                                    this.NysRp525.controls[propNameWithCapitalFirstLetter].setValue(
                                        ['true', 'false'].includes(barReviewAnswers[propName])
                                            ? barReviewAnswers[propName] === 'true'
                                            : barReviewAnswers[propName]
                                    );
                                }
                                this.NysRp525.controls.SignatureAsBase64String.setValue(undefined);
                            },
                            (err) => {
                                console.error(err);
                                window.alert('An error occurred. Please retry.');
                            }
                        );
                },
                (error) => {
                    console.error(error);
                    window.alert('An error occurred. Please retry.');
                },
                () => {
                    // finally
                }
            );
        });
    }

    public ngOnInit(): void {
        this.canvas = document.getElementById('Admin_BarReview_sig-pad-canvas');
        this.signaturePad = new SignaturePad(
            this.canvas,
            {
                // It's Necessary to use an opaque color when saving image as JPEG;
                // this option can be omitted if only saving as PNG or SVG
                backgroundColor: 'rgb(255, 255, 255)'
            }
        );
        // On mobile devices it might make more sense to listen to orientation change,
        // rather than window resize events.
        window.onresize = () => {
            try { this.resizeCanvas(); }
            catch {}
        };
    }

    /** Uses instance data to prefill the form */
    private prefillRp525(): void {
        // The user will likely perform this repeatedly, and we want to minimize their work,
        this.NysRp525.controls.Admin_Rp525_SignDate.setValue(this.signatureDateCache);
        this.NysRp525.controls.Admin_Rp525_SignName.setValue(this.signatureNameCache);

        this.NysRp525.controls.Admin_Rp525_Muni.setValue(this.rp524Data.Muni);

        const possibleCarriageReturn =
            (this.rp524Data.OwnerNameLine2?.length > 0)
                ? '\n' : '';
        const ownerNameAndAddress =
            `${this.rp524Data.OwnerNameLine1 ?? ''} ${possibleCarriageReturn}${this.rp524Data.OwnerNameLine2 ?? ''} \n` +
            `${this.rp524Data.OwnerAddressLine1 ?? ''} \n` +
            `${this.rp524Data.OwnerAddressLine2 ?? ''} ${this.rp524Data.OwnerAddressLine3 ?? ''}`;
        this.NysRp525.controls.Admin_Rp525_ComplainantInfoTextArea.setValue(ownerNameAndAddress);

        this.NysRp525.controls.Admin_Rp525_TaxMapId.setValue(this.rp524Data.TaxMapNum);
        this.NysRp525.controls.Admin_Rp525_Location1.setValue(this.rp524Data.LocationStreetAddress);
        this.NysRp525.controls.Admin_Rp525_Location2.setValue(this.rp524Data.LocationCityTown);

        this.NysRp525.controls.Admin_Rp525_Tentative.setValue(this.rp524Data.TotalVal);
        this.NysRp525.controls.Admin_Rp525_Land.setValue('N\\A');
        this.NysRp525.markAllAsTouched();
    }

    /** Used as a fallback method of obtaining some data for prefilling */
    private prefillRp525FromGridData(): void {
        const fallbackData: IAssessmentGrievance = this.parent.GetSelectedData()[0];
        // The user will likely perform this repeatedly, and we want to minimize their work,
        this.NysRp525.controls.Admin_Rp525_SignDate.setValue(this.signatureDateCache);
        this.NysRp525.controls.Admin_Rp525_SignName.setValue(this.signatureNameCache);

        const ownerNameAndAddress =
            `${fallbackData.complainant ?? ''} \n` +
            `${fallbackData.complainant_mail_address ?? ''} `;

        this.NysRp525.controls.Admin_Rp525_ComplainantInfoTextArea.setValue(ownerNameAndAddress);

        this.NysRp525.controls.Admin_Rp525_TaxMapId.setValue(fallbackData.tax_map_id);
        // this.NysRp525.controls.Admin_Rp525_Location1.setValue(fallbackData.LocationStreetAddress);
        // this.NysRp525.controls.Admin_Rp525_Location2.setValue(fallbackData.LocationCityTown);

        // this.NysRp525.controls.Admin_Rp525_Tentative.setValue(fallbackData.TotalVal);
        this.NysRp525.controls.Admin_Rp525_Land.setValue('N\\A');
        this.NysRp525.markAllAsTouched();
    }

    /** Intended to be used as a fallback. */
    public GetPrefillDataFromExternalService() {
        // NOTE: We don't have to check the truthiness of the the TaxMapId below
        // because it's part of a hidden form set up from user's previous actions.
        this.IsFetchingPrefillData = true;
        this.http.GetRemoteRp525PrefillData(this.NysRp525.controls.TaxMapId.value).subscribe(
            (res: IExternalService525PrefillData) => {
                console.warn(res);
                this.rp524Data.LocationCityTown = res.LocationCityTown;
                this.rp524Data.LocationCounty = res.LocationCounty;
                this.rp524Data.LocationStreetAddress = res.LocationStreetAddress;
                this.rp524Data.LocationVillage = res.LocationVillage;
                this.rp524Data.Muni = res.Muni;
                this.rp524Data.OwnerAddressLine1 = res.OwnerAddressLine1;
                this.rp524Data.OwnerAddressLine2 = res.OwnerAddressLine2;
                this.rp524Data.OwnerNameLine1 = res.OwnerNameLine1;
                this.rp524Data.OwnerNameLine2 = res.OwnerNameLine2;
                this.rp524Data.TaxMapNum = res.TaxMapNum;
                this.rp524Data.TotalVal = res.TotalVal;

                this.prefillRp525();

                this.IsFetchingPrefillData = false;
            },
            (err) => {
                console.error(err);
                window.alert(
                    'Unable to fetch data. Please retry.'
                );
                this.IsFetchingPrefillData = false;
            }
        );
    }

    private displayImage(signatureDataUrl: string): void {
        this.NysRp525.controls.SignatureAsBase64String.setValue(
            this.formData.GetBase64ImageFromDataUrl(signatureDataUrl)
        );
        document.getElementById('Admin_BarReview_SignatureInput').setAttribute('src', signatureDataUrl);
        document.getElementById('Admin_BarReview_SignatureUnderline').setAttribute('style', 'height: 5px;');
    }

    private signatureSaveHandler(canvas = this.canvas): void {
        trimCanvas(canvas);
        const dataURL = this.signaturePad.toDataURL();
        this.displayImage(dataURL);

        // Hide signature pad
        // this.signaturePadWrapper.setAttribute('style', 'display: none;');
        this.IsSignaturePadHidden = true;

        // Autofill signature date
        this.NysRp525.controls.Admin_Rp525_SignDate.setValue(
            new Date().toLocaleDateString('en-US')
        );
    }

    public ShowSignaturePad() {
        this.IsSignaturePadHidden = false;

        // NOTE: SignaturePad won't work in a BS modal if you don't do this.
        // OTHER NOTE: We must wait some amount of time for Angular to remove a class (TODO)
        const ms = 250;
        window.setTimeout(
            () => { window.dispatchEvent(new Event('resize')); },
            ms
        );
    }

    public ClearSignature(id: string = 'Admin_BarReview_SignatureInput') {
        document.getElementById(id).setAttribute('src', '');
        this.NysRp525.controls.Admin_Rp525_SignDate.setValue('');
    }

    public InitiateSignatureFileDialog() {
        document.getElementById('Admin_BarReview_SignatureFile').click();
        this.signatureSaveHandler();
    }

    /** Erase the signature from the signature-pad canvas. */
    public EraseSignatureFromSignaturePad() {
        this.signaturePad.clear();
    }

    /** "Save" the signature from UI widget to another. */
    public SaveSignature() {
        this.signatureSaveHandler();
    }

    /** TODO: Unit test */
    private validateUploadInputs(
        msgFunc = window.alert,
        signature = this.NysRp525.controls.SignatureAsBase64String
    ): boolean {
        if (!signature || !signature.value || signature.value.length === 0) {
            msgFunc(
                'It appears you haven\'t added a signature. A signature is required.'
            );
            return false;
        }
        if (this.NysRp525.invalid === true) {
            console.warn('The following inputs are invalid:');
            for (const input in this.NysRp525.controls) {
                if (this.NysRp525.controls[input].status !== 'VALID') {
                    console.warn('Name: ' + input);
                    console.warn('Value: ' + this.NysRp525.controls[input].value);
                }
            }
            window.alert(
                'The form is invalid. Please review the error messages displayed on the form.'
            );
            // TODO: element.focus() on the first issue

            return false;
        }
        // TODO: Check that XOR All Concur or All ConcurExcept?
        return true;
    }

    public async SaveOnlineBarReview(displayCompletionMessage: boolean = true) {
        this.IsOnlineBarReviewSaving = true;
        try {
            await this.http.SaveOnlineBarReview(this.NysRp525.value);
            if (displayCompletionMessage === true) {
                window.alert('Review data was saved.');
            }
        }
        catch (err) {
            if (displayCompletionMessage === true) {
                window.alert('An error occurred during saving. Please retry.');
            }
            else {
                throw err;
            }
        }
        finally {
            this.IsOnlineBarReviewSaving = false;
        }
    }

    public async CompleteOnlineBarReview() {
        if (this.validateUploadInputs() === false) { return; }
        try {
            await this.SaveOnlineBarReview(false);
            this.IsOnlineBarReviewUploading = true;
            const response = await this.http.UploadOnlineBarReviewData(this.NysRp525.value);
            if (response.ok) {

                // We avoid leaving stale data in the form, but cache some data for convenience.
                const formInputs = this.NysRp525.controls;
                this.signatureDateCache = formInputs.Admin_Rp525_SignDate.value;
                this.signatureNameCache = formInputs.Admin_Rp525_SignName.value;

                this.ClearSignature();
                this.NysRp525.controls.SignatureAsBase64String.setValue(undefined);
                this.NysRp525.reset();

                // Reset file input
                // @ts-ignore
                document.getElementById('nysRp525File').value = '';

                formInputs.Admin_Rp525_SignDate.setValue(this.signatureDateCache);
                formInputs.Admin_Rp525_SignName.setValue(this.signatureNameCache);

                // TODO: wrap behavior below into the back-end controller action?
                // this.parent.ToggleBarReviewedStatus(true);

                window.alert(
                    'Review completed successfully. The grievance was marked as being BAR-Reviewed. \n\n' +
                    'In addition, a completed NYS RP-525 file \n' +
                    'was added to the application package in cloud storage.'
                );

                $(`#${this.barReviewModalId}`).modal('hide');
                return;
            }
            else if (response.status === 403) {
                window.alert('You are not authorized to perform this action.');
            }
            else if (response.status === 500) {
                window.alert('An error occurred. Please retry.');
            }
            else {
                console.error(response);
                window.alert('An unknown issue occurred. Please retry.');
            }
        }
        catch (error) {
            window.alert(
                'An error occurred while making a network request. Please retry.'
            );
        }
        finally {
            this.IsOnlineBarReviewUploading = false;
        }
    }

    /**
     * @param signatureFileInputId TODO: Refactor to not need this ID
     */
    public RenderSignatureImage(signatureFileInputId: string = 'Admin_BarReview_SignatureFile') {
        const selector = `#${signatureFileInputId}`;
        if (
            !document.querySelector(selector)
            ||
            // @ts-ignore
            document.querySelector(selector).value === ''
        ) {
            return console.log('No file selected');
        }
        // @ts-ignore
        const file = document.querySelector(selector).files[0];
        const reader = new FileReader();
        reader.onload = (e) => {
            // @ts-ignore
            const dataUrl: string = e.target.result;
            this.displayImage(dataUrl);
            this.IsSignaturePadHidden = true;
        };
        reader.onerror = (e) => {
            this.IsSignaturePadHidden = true;
            window.alert(
                'An error occurred while loading your signature file. \n' +
                'The file must be one of the following types: JPEG, PNG, or BMP. \n' +
                'Please retry with a file of the appropriate type.'
            );
            console.error(e);
        };
        reader.readAsDataURL(file);
    }

    // Adjust canvas coordinate space taking into account pixel ratio,
    // to make it look crisp on mobile devices.
    // This also causes canvas to be cleared.
    // When zoomed out to less than 100%, for some very strange reason,
    // some browsers report devicePixelRatio as less than 1
    // and only part of the canvas is cleared then.
    private resizeCanvas(): void {
        const ratio = Math.max(window.devicePixelRatio || 1, 1);
        // This part causes the canvas to be cleared
        this.canvas.width = this.canvas.offsetWidth * ratio;
        this.canvas.height = this.canvas.offsetHeight * ratio;
        this.canvas.getContext('2d').scale(ratio, ratio);
        // This library does not listen for canvas changes, so after the canvas is automatically
        // cleared by the browser, SignaturePad#isEmpty might still return false, even though the
        // canvas looks empty, because the internal data of this library wasn't cleared. To make sure
        // that the state of this library is consistent with visual state of the canvas, you
        // have to clear it manually.
        this.signaturePad.clear();
    }

    public HandleSignatureKeyup(event: KeyboardEvent) {
        this.signaturePad.clear();
        const userName: string = (event.target as HTMLInputElement).value;
        const ctx = this.canvas.getContext('2d');
        ctx.font = '70px Meie Script';
        // ctx.textAlign = 'center';
        ctx.fillText(userName, 10, 60);
        // this.canvas = this.trimCanvas(this.canvas);
        // TODO: Finger print user (IP, user agent et cetera)
    }

}
