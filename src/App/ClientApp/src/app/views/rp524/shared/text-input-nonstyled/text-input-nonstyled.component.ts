import { Component, OnInit, Input, AfterViewInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { IFormElement } from 'src/app/types/IFormElement';
import { CurrencyPipe } from '@angular/common';

@Component({
    selector: 'app-text-input-nonstyled',
    templateUrl: './text-input-nonstyled.component.html',
    styleUrls: ['./text-input-nonstyled.component.css']
})
export class TextInputNonstyledComponent implements OnInit, AfterViewInit, IFormChildComponent, IFormElement {

    @Input()
    public readonly Id: string;

    @Input()
    public readonly Classes: string = 'width-fifty py-0';

    @Input()
    public readonly Placeholder: string;

    @Input()
    public readonly ParentForm: FormGroup;

    @Input()
    public readonly IsCurrency: boolean = false;

    @Input()
    public readonly IsRequired: boolean;

    @ViewChild('textInput')
    private input: any;

    public readonly FormControl: FormControl = new FormControl('');

    constructor(private readonly currencyPipe: CurrencyPipe) {
        //
    }

    public ngOnInit(): void {
        this.ParentForm.addControl(this.Id, this.FormControl);
    }

    public ngAfterViewInit() {
        if (this.IsRequired === true) {
            this.input.nativeElement.setAttribute('required', 'true');
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
        // Make elements in part 6 readonly
        // TODO: This will be a problem if we ever sell the app
        if (
            this.Id === 'six_two_text'
            ||
            this.Id === 'six_three_text'
        ) {
            this.input.nativeElement.readOnly = true;
        }
    }

    /**
     * TODO: Refactor to shared extraction
     */
    private transformAmount(value: any){
        return this.currencyPipe.transform(value, 'USD', 'symbol', '1.0-0');
    }
}
