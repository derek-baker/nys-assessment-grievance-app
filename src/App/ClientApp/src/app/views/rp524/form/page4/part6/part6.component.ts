import { Component, OnInit, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';

@Component({
    selector: 'app-part6',
    templateUrl: './part6.component.html',
    styleUrls: ['./part6.component.css']
})
export class Part6Component implements OnInit, IFormChildComponent { // , ISignable

    @Input()
    public readonly ParentForm: FormGroup;

    constructor() {
        //
    }

    public ngOnInit(): void {
        //
    }
}
