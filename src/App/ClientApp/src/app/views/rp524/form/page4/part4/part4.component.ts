import { Component, OnInit, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import 'bootstrap';
// @ts-ignore
import $ from 'jquery';
import SignaturePad from 'signature_pad';
import trimCanvas from 'trim-canvas';
import { ISignable } from 'src/app/types/ISignable';
import { FormDataService } from 'src/app/services/form-data.service';

@Component({
    selector: 'app-part4',
    templateUrl: './part4.component.html',
    styleUrls: ['./part4.component.css']
})
export class Part4Component implements OnInit, IFormChildComponent, ISignable {

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
        this.wrapper = document.getElementById('signature-pad-part4');
        this.clearButton = this.wrapper.querySelector('[data-action=clear]');
        this.saveSignatureButton = this.wrapper.querySelector('[data-action=save-png]');
        this.canvas = document.getElementById('sig-pad-canvas-part4');
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
                // NOTE: We could come up with a better way to write an event handler that
                //       resizes the canvas only when visible, or just do this.
            }
        };

        this.resizeCanvas();
        this.clearButton.addEventListener('click', () => {
            this.signaturePad.clear();
        });

        const displayImage = (signatureDataUrl: string) => {
            // TODO: refactor out?
            this.formData.SignatureFromPart4 = this.formData.GetBase64ImageFromDataUrl(signatureDataUrl);

            document.getElementById('signature1').setAttribute('src', signatureDataUrl);
            document.getElementById('signatureUnderline1').setAttribute('style', 'height: 5px;');
        };

        const signatureSaveHandler = (controlName: string = 'four_date_text') => {
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
        document.getElementById('UploadSignature').addEventListener(
            'click',
            () => {
                signatureSaveHandler();
            }
        );

        // NOTE: SignaturePad won't work in a BS modal if you don't do this.
        // document.querySelector('#SignatureModal').addEventListener('shown.bs.modal', () => {
        $('#SignatureModal').on('shown.bs.modal', () => {
            this.resizeCanvas();
        });

        document.querySelector('#UploadSignature').addEventListener('click', () => {
            document.getElementById('SignatureFile').click();
        });

        this.initSignatureFileListeners(displayImage);
    }

    /** TODO: Refactor out */
    private initSignatureFileListeners(
        displayImageFunc: (signatureDataUrl: string, el1Id?: string, el2Id?: string) => void,
        signatureFileInputId: string = 'SignatureFile',
        signatureModalId: string = 'SignatureModal'
    ) {
        const selector = `#${signatureFileInputId}`;
        document.querySelector(selector).addEventListener('change', () => {
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
                displayImageFunc(dataUrl);
                $(`#${signatureModalId}`).modal('hide');
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
        // tslint:disable-next-line: no-string-literal
        this.ParentForm.controls['four_date_text'].setValue('');
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

    // private trimCanvas(canvas: any) {
    //     const context = canvas.getContext('2d');
    //     const topLeft = {
    //         x: canvas.width,
    //         y: canvas.height,
    //         update(x, y) {
    //             this.x = Math.min(this.x, x);
    //             this.y = Math.min(this.y, y);
    //         }
    //     };
    //     const bottomRight = {
    //         x: 0,
    //         y: 0,
    //         update(x, y) {
    //             this.x = Math.max(this.x, x);
    //             this.y = Math.max(this.y, y);
    //         }
    //     };
    //     const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
    //     for (let x = 0; x < canvas.width; x++) {
    //         for (let y = 0; y < canvas.height; y++) {
    //             const alpha = imageData.data[((y * (canvas.width * 4)) + (x * 4)) + 3];
    //             if (alpha !== 0) {
    //                 topLeft.update(x, y);
    //                 bottomRight.update(x, y);
    //             }
    //         }
    //     }
    //     const width = bottomRight.x - topLeft.x;
    //     const height = bottomRight.y - topLeft.y;
    //     const croppedCanvas = context.getImageData(topLeft.x, topLeft.y, width, height);
    //     canvas.width = width;
    //     canvas.height = height;
    //     context.putImageData(croppedCanvas, 0, 0);
    //     return canvas;
    // }
}
