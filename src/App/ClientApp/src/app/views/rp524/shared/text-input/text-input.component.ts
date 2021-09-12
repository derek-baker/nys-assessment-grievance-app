import { Component, OnInit, Input, ViewChild, AfterViewInit } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { IFormElement } from 'src/app/types/IFormElement';
import { CurrencyPipe } from '@angular/common';

@Component({
    selector: 'app-text-input',
    templateUrl: './text-input.component.html',
    styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements OnInit, AfterViewInit, IFormChildComponent, IFormElement {

    @Input()
    public readonly ParentForm: FormGroup;

    /** Should correspond to a prop in IPrefillData */
    @Input()
    public readonly Id: string;

    @Input()
    public readonly IsRequired: boolean;

    @Input()
    public readonly MinLength: number;

    @Input()
    public readonly MaxLength: number;

    @Input()
    public readonly HasBorder: boolean;

    @Input()
    public readonly Placeholder: string;

    @Input()
    public readonly IsCurrency: boolean = false;

    public readonly FormControl: FormControl = new FormControl('');

    @ViewChild('textInput')
    private input: any;

    constructor(
        private readonly currencyPipe: CurrencyPipe
    ) {
        //
    }

    public ngOnInit() {
        this.ParentForm.addControl(this.Id, this.FormControl);
    }

    public ngAfterViewInit() {
        if (this.IsRequired === true) {
            this.FormControl.setValidators([Validators.required]);
        }
        if (this.HasBorder === false) {
            this.input.nativeElement.classList.add('noBorder');
        }
        if (this.IsCurrency === true) {
            this.input.nativeElement.addEventListener(
                'input',
                () => {
                    this.FormControl.setValue(
                        this.transformAmount(
                            this.FormControl.value.replace('.00', '').replace('.0', '').replace(/\D/g, '')
                        )
                    );
                }
            );
        }
    }

    private transformAmount(value: any) {
        return this.currencyPipe.transform(value, 'USD', 'symbol', '1.0-0');
    }
}
