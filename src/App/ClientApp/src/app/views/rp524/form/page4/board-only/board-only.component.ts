import { Component, OnInit, Output, EventEmitter, AfterViewInit } from '@angular/core';

@Component({
    selector: 'app-board-only',
    templateUrl: './board-only.component.html',
    styleUrls: ['./board-only.component.css']
})
export class BoardOnlyComponent implements OnInit, AfterViewInit {

    @Output()
    public formLoaded = new EventEmitter<boolean>();

    @Output()
    public completeRp524Event = new EventEmitter();

    constructor() {
        //
    }

    public ngOnInit(): void {
        //
    }

    public ngAfterViewInit() {
        console.warn('Emitting formLoaded event');
        this.formLoaded.emit(true);
    }

    public CompleteRp524() {
        this.completeRp524Event.emit(null);
    }

    // public ScrollToTop(browserWindow: Window = window) {
    //     browserWindow.scroll({
    //         top: 0,
    //         left: 0,
    //         behavior: 'smooth'
    //     });
    // }
}
