import { Component, OnInit, Input } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'app-part3',
    templateUrl: './part3.component.html',
    styleUrls: ['./part3.component.css']
})
export class Part3Component implements OnInit, IFormChildComponent {

    @Input()
    public readonly ParentForm: FormGroup;

    constructor() {
        //
    }

    public ngOnInit(): void {
        //
    }
}
