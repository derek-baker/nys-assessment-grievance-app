import { Injectable } from '@angular/core';
import { HttpService } from './http.service';

@Injectable({
    providedIn: 'root'
})
export class TimelineValidationService {

    private submissionsDateStart: Date;
    private submissionsDateEnd: Date;
    private supportingDocsDateEnd: Date;

    constructor(private readonly httpService: HttpService) {
        this.httpService.GetTimelineSetting().subscribe(
            (result: any) => {
                const timeline: {submissionsDateStart: string, submissionsDateEnd: string, supportingDocsDateEnd: string} = result;

                this.submissionsDateStart = new Date(timeline.submissionsDateStart);
                this.submissionsDateEnd = new Date(timeline.submissionsDateEnd);
                this.supportingDocsDateEnd = new Date(timeline.supportingDocsDateEnd);
            },
            (error) => {
                console.error(error);
            }
        );
     }

    public TestInitialSubmissionDateValidity(
        submissionsDateStart: Date = this.submissionsDateStart,
        submissionsDateEnd: Date = this.submissionsDateEnd,
        dateNow: Date = new Date(),
        alertFunc: (message?: any) => void = window.alert
    ): boolean {
        const msg = 'Param submissionsDateStart cannot be before submissionsDateEnd';
        if (submissionsDateStart > submissionsDateEnd) {
            alertFunc('ERROR: ' + msg);
            throw new Error(msg);
        }

        if (dateNow < submissionsDateStart) {
            alertFunc(
                'The time period for submissions has not started yet. \n' +
                `The application will begin accepting submissions on ${submissionsDateStart.toLocaleDateString('en-US')}`
            );
            return false;
        }
        if (dateNow > submissionsDateEnd) {
            alertFunc(
                'The deadline for submissions has passed. \n' +
                `It ended on ${submissionsDateEnd.toLocaleDateString('en-US')}. \n` +
                'You\'ll have another opportunity to grieve your assessment next year.'
            );
            return false;
        }
        return true;
    }

    public TestSupportingDocsUploadDateValidity(
        submissionsDateStart: Date = this.submissionsDateStart,
        supportingDocsDateEnd: Date = this.supportingDocsDateEnd,
        dateNow: Date = new Date(),
        alertFunc: ((message?: any) => void) = window.alert
    ): boolean {
        const msg = 'Param submissionsDateStart cannot be before supportingDocsDateEnd';
        if (submissionsDateStart > supportingDocsDateEnd) {
            alertFunc('ERROR: ' + msg);
            throw new Error(msg);
        }

        if (submissionsDateStart > supportingDocsDateEnd) {
            throw new Error('Param submissionsDateStart cannot be before supportingDocsDateEnd');
        }
        if (dateNow < submissionsDateStart) {
            alertFunc(
                'The time period for submissions has not started yet. \n' +
                `The application will begin accepting submissions on ${submissionsDateStart.toLocaleDateString('en-US')}`
            );
            return false;
        }
        if (dateNow > supportingDocsDateEnd) {
            alertFunc(
                'The deadline for submissions has passed. \n' +
                `It ended on ${supportingDocsDateEnd.toLocaleDateString('en-US')}. \n` +
                'You\'ll have another opportunity to grieve your assessment next year.'
            );
            return false;
        }
        return true;
    }
}
