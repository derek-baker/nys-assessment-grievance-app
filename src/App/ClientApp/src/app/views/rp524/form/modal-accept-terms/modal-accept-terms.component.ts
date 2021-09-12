import { Component, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-modal-accept-terms',
    templateUrl: './modal-accept-terms.component.html',
    styleUrls: ['./modal-accept-terms.component.css']
})
export class ModalAcceptTermsComponent implements OnInit {

    public readonly RequirementsSatisfied = {
        RequirementOne: false,
        RequirementTwo: false
    };

    @Output()
    private acceptTermsEvent = new EventEmitter<string>();

    constructor() { }

    public ngOnInit(): void {}

    private validateRequirements(reqs = this.RequirementsSatisfied): boolean {
        for (const prop in reqs) {
            if (reqs[prop] === false) {
                return false;
            }
        }
        return true;
    }

    public CloseTerms() {
        if (this.validateRequirements() === true) {
            this.acceptTermsEvent.emit();
            return;
        }
        window.alert(
            'Please review the first section of this modal. \n' +
            'You\'re required to download and read a file, and check a box.'
        );
    }
}
