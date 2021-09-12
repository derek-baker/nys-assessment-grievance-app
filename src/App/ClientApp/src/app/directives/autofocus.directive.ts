import { Directive, ElementRef, OnInit } from '@angular/core';

@Directive({
    selector: '[appAutofocus]'
})
export class AutofocusDirective implements OnInit {

    constructor(private el: ElementRef) {
        // tslint:disable-next-line: no-string-literal
        if (!el.nativeElement['focus']) {
            throw new Error('Element does not accept focus.');
        }
    }

    public ngOnInit(): void {
        const input: HTMLInputElement = this.el.nativeElement as HTMLInputElement;
        input.focus();
        input.select();
    }
}
