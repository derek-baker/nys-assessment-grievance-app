import { Component, OnInit, Input } from '@angular/core';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'app-part1',
    templateUrl: './part1.component.html',
    styleUrls: ['./part1.component.css']
})
export class Part1Component implements OnInit, IFormChildComponent {

    @Input()
    public readonly ParentForm: FormGroup;

    constructor() {
        //
    }

    public ngOnInit(): void {

    }
}
