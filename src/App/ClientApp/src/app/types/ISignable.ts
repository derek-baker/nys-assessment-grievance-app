export interface ISignable {
    wrapper: any;
    clearButton: any;
    saveSignatureButton: any;
    canvas: any;
    signaturePad: any;

    resizeCanvas(): void;

    ClearSignature(id: string): void;
}
