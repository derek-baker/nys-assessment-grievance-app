import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'app-spinner',
    templateUrl: './spinner.component.html',
    styleUrls: ['./spinner.component.css']
})
export class SpinnerComponent implements OnInit {

    @Input()
    public readonly Id: string;
    @Input()
    public readonly SelectedId: string;
    @Input()
    public readonly IsSpinning: boolean;

    public get IsThisSpinning() {
        return this.IsSpinning === true
            && this.Id
            && this.Id === this.SelectedId;
    }

    constructor() {}

    public ngOnInit() {}
}
