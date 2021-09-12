import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { IFormChildComponent } from 'src/app/types/IFormChildComponent';

@Component({
    selector: 'app-part2',
    templateUrl: './part2.component.html',
    styleUrls: ['./part2.component.css']
})
export class Part2Component implements OnInit, IFormChildComponent {

    @Input()
    public readonly ParentForm: FormGroup;

    constructor() {
        //
    }

    public ngOnInit(): void {
        //
    }
}
