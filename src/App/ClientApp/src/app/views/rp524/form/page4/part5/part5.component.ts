import { Component, OnInit, Input } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { FormGroup } from '@angular/forms';
// @ts-ignore
import $ from 'jquery';
import SignaturePad from 'signature_pad';
import trimCanvas from 'trim-canvas';
import { ISignable } from 'src/app/types/ISignable';
import { FormDataService } from 'src/app/services/form-data.service';
import { SignatureType } from 'src/app/types/enums/SignatureType';

@Component({
    selector: 'app-part5',
    templateUrl: './part5.component.html',
    styleUrls: ['./part5.component.css']
})
export class Part5Component implements OnInit, IFormChildComponent, ISignable {

    private readonly signatureTypeControlName: string = 'five_signature_type';

    @Input()
    public readonly ParentForm: FormGroup;

    public wrapper: any;
    public clearButton: any;
    public saveSignatureButton: any;
    public canvas: any;
    public signaturePad: any;

    constructor(private readonly formData: FormDataService) {
        //
    }

    public ngOnInit(): void {
        this.wrapper = document.getElementById('signature-pad-part5');
        this.clearButton = this.wrapper.querySelector('[data-action=clear]');
        this.saveSignatureButton = this.wrapper.querySelector('[data-action=save-png]');
        this.canvas = document.getElementById('sig-pad-canvas-part5');
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
        // window.onresize = this.resizeCanvas;
        window.onresize = () => {
            try {
                this.resizeCanvas();
            }
            catch {
                //
            }
        };

        this.resizeCanvas();

        this.clearButton.addEventListener('click', () => {
            this.signaturePad.clear();
        });

        const displayImage = (
            signatureDataUrl: string,
            el1Id = 'signature2',
            el2Id = 'signatureUnderline2'
        ) => {
            this.formData.SignatureFromPart5 = this.formData.GetBase64ImageFromDataUrl(signatureDataUrl);
            document.getElementById(el1Id).setAttribute('src', signatureDataUrl);
            document.getElementById(el2Id).setAttribute('style', 'height: 5px;');
        };

        const signatureSaveHandler = (controlName: string = 'five_date_text') => {
            trimCanvas(this.canvas);
            const dataURL = this.signaturePad.toDataURL();
            displayImage(dataURL);
            // tslint:disable-next-line: no-string-literal
            this.ParentForm.controls[controlName].setValue(
                new Date().toLocaleDateString('en-US')
            );
        };
        this.saveSignatureButton.addEventListener(
            'click',
            () => {
                signatureSaveHandler();
            }
        );
        document.getElementById('UploadSignature2').addEventListener(
            'click',
            () => {
                signatureSaveHandler();
            }
        );

        // NOTE: SignaturePad won't work in a BS modal if you don't do this.
        $('#SignatureModalPart5').on('shown.bs.modal', () => {
            this.resizeCanvas();
        });

        document.querySelector('#UploadSignature2').addEventListener('click', () => {
            document.getElementById('SignatureFile2').click();
        });

        this.initSignatureFileListeners(displayImage);
    }

    private initSignatureFileListeners(
       displayImageFunc: (signatureDataUrl: string, el1Id?: string, el2Id?: string) => void,
       signatureFileInputId: string = 'SignatureFile2',
       signatureModalId: string = 'SignatureModalPart5'
    ) {
        const selector = `#${signatureFileInputId}`;
        document.querySelector(selector).addEventListener('change', () => {
            if (
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
                displayImageFunc(dataUrl);
                $(`#${signatureModalId}`).modal('hide');
                this.recordSignatureType(SignatureType.Uploaded);
            };
            reader.onerror = (e) => {
                window.alert(
                    'An error occurred while loading your signature file. \n' +
                    'The file must be one of the following types: JPEG, PNG, or BMP. \n' +
                    'Please retry with a file of the appropriate type.'
                );
                console.error(e);
            };
            reader.readAsDataURL(file);
        });
    }

    private recordSignatureType(signatureType: SignatureType) {
        this.ParentForm.controls[this.signatureTypeControlName].setValue(
            signatureType
        );
    }

    public HandleSignatureKeyup(event: KeyboardEvent) {
        this.signaturePad.clear();

        const userName: string = (event.target as HTMLInputElement).value;
        const ctx = this.canvas.getContext('2d');

        ctx.font = '70px Meie Script';
        ctx.fillText(userName, 10, 60);

        this.recordSignatureType(SignatureType.Typed);
        // TODO: Finger print user (IP, user agent et cetera)
    }

    // Adjust canvas coordinate space taking into account pixel ratio,
    // to make it look crisp on mobile devices.
    // This also causes canvas to be cleared.
    // When zoomed out to less than 100%, for some very strange reason,
    // some browsers report devicePixelRatio as less than 1
    // and only part of the canvas is cleared then.
    public resizeCanvas(): void {
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

    public ClearSignature(id: string) {
        document.getElementById(id).setAttribute('src', '');
        this.ParentForm.controls.five_date_text.setValue('');
    }
}
