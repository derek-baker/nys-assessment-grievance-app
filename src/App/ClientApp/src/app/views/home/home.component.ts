import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html'
})
export class HomeComponent {
    public SubmissionId: string;

    constructor(private readonly router: Router) {
        this.SubmissionId = this.router.getCurrentNavigation()?.extras?.state?.grievanceId;
    }
}
