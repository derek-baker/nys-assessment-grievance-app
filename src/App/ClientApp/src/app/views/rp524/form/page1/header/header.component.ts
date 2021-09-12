import { Component, OnInit, Input } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { FormGroup } from '@angular/forms';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'form-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, IFormChildComponent {

    @Input()
    public readonly ParentForm: FormGroup;

    constructor() {
        //
    }

    public ngOnInit(): void {
        //
    }
}
