import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Injectable({
    providedIn: 'root'
})
export class FormDataService {

    /** This should be the RP524 data the user fills in the browser. */
    private form: FormGroup;

    /** Used as a hacky way to determine if we should silence data-loss warnings */
    public UserWantsToSubmit: boolean = false;
    public Email: string;
    public TaxMapId: string;

    /** Data extracted from Angular reactive form */
    public FormData: any;

    /** Base64 representation of signature image */
    public SignatureFromPart4: string;

    /** Base64 representation of signature image */
    public SignatureFromPart5: string;

    constructor() {
        //
    }

    public InitFormRef(formRef: FormGroup): void {
        // if (this.form) {
        //     throw new Error(
        //         'ERROR: this.form is already initialized. Please investigate.'
        //     );
        // }
        this.form = formRef;
    }

    public SetFormValues(data: any): void {
        // tslint:disable-next-line: forin
        for (const prop in data) {
            this.form.controls[prop].setValue(data[prop]);
            this.form.controls[prop].enable();
        }
    }

    public GetFormValue(key: string): string {
        return this.form.controls[key].value;
    }

    /** Note that the current implementation of this method depends on checkboxes containing 'check'. */
    public ExtractFormValues(): any {
        const formValues = {};
        // tslint:disable-next-line: forin
        for (const propName in this.form.controls) {
            if (propName.includes('check') || propName.includes('Check')) {
                formValues[propName] =
                    (this.form.controls[propName].value === true)
                        ? true
                        : false;
            }
            else {
                formValues[propName] = this.form.controls[propName].value;
            }

        }
        return formValues;
    }

    /**
     * TODO: I think this breaks on Legacy Edge
     * DOCS: https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/Data_URIs
     * @param dataURL - Assumed to be in this format: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAO0AAAAWCAYAAAAsGBtVAAADV'
     */
    public GetBase64ImageFromDataUrl(dataURL: string): string {
        // const base64Img = dataURL.split(';base64,')[1];
        // return base64Img;

        const urlSections: Array<string> = dataURL.split(';base64,');
        if (urlSections.length !== 2) {
            return '';
        }
        const base64Img = urlSections[1];
        return base64Img;
    }
}
