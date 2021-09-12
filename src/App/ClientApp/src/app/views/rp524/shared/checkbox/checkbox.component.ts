import { Component, OnInit, Input } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { IFormElement } from 'src/app/types/IFormElement';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
    selector: 'app-checkbox',
    templateUrl: './checkbox.component.html',
    styleUrls: ['./checkbox.component.css']
})
export class CheckboxComponent implements OnInit, IFormChildComponent, IFormElement {

    @Input()
    public readonly Id: string;

    @Input()
    public readonly ParentForm: FormGroup;

    public readonly FormControl: FormControl = new FormControl('');

    constructor() {
        //
    }

    public ngOnInit(): void {
        this.ParentForm.addControl(this.Id, this.FormControl);
    }
}
