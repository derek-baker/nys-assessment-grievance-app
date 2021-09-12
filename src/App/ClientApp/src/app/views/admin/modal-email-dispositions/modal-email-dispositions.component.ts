import { Component, OnInit, Input } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { IAssessmentGrievance } from 'src/app/types/IAssessmentGrievance';
import { HttpService } from 'src/app/services/http.service';
import { Resources } from '../Resources';
import { SortService } from 'src/app/services/sort.service';
import { HttpPublicService } from 'src/app/services/http.service.public';
import { IAttorneyPrefillData } from 'src/app/types/IAttorneyPrefillData';

@Component({
    selector: 'app-modal-email-dispositions',
    templateUrl: './modal-email-dispositions.component.html',
    styleUrls: ['./modal-email-dispositions.component.css']
})
export class ModalEmailDispositionsComponent implements OnInit {

    @Input()
    public readonly UserName: string;
    @Input()
    public readonly Password: string;

    public IsTriggeringJob: boolean = false;

    public EmailJobQueueData: Array<IEmailJobQueueData> = [];

    /** Used to track value in <select> */
    public readonly RepGroupSelectControl: FormControl = new FormControl();
    /** Used to populate <select> */
    public SubmissionsAndCompletions: Array<ISubmissionsPerEmail> = [];
    /** Used to filter available <options> in a <select> */
    public MinNumSubmissionsPredicate: number = 1;

    private attorneyData: Array<IAttorneyPrefillData> = [];

    private selectedRepGroups: Array<string> = [];
    public get GetSelectedRepGroups(): Array<string> { return this.selectedRepGroups; }
    public set SetSelectedRepGroups(selectedRepGroups: Array<string>) {
        this.selectedRepGroups = selectedRepGroups;
    }

    private submissionsPerEmail: Array<ISubmissionsPerEmail>;
    private submissionsPerEmailCopy: Array<ISubmissionsPerEmail>;

    private completionsPerEmail: Map<string, number>;
    public GetCompletionsPerEmails() {
        if (!this.completionsPerEmail) { return new Map<string, number>(); }
        return this.completionsPerEmail;
    }
    public set SetCompletionsPerEmail(data: Array<IAssessmentGrievance>) {
        const countReviewCompletionsPerEmail = (
           grievanceData: Array<IAssessmentGrievance>
        ): Map<string, number> => {
           const counts: Map<string, number> = new Map();

           grievanceData.forEach(
               (el) => {
                   const repGroupInfo: {GroupName1: string, Email: string | null} =
                       this.attorneyData.find(
                           (x) => x.GroupName1 === el.attorney_group
                       );

                   const emailLowerCase = (repGroupInfo && repGroupInfo.Email)
                       ? repGroupInfo.Email.toLowerCase()
                       : el.email.toLowerCase();

                   if (counts.has(emailLowerCase) === true && el.barReviewed === true) {
                       counts.set(emailLowerCase, (counts.get(emailLowerCase) + 1));
                   }
                   else if (counts.has(emailLowerCase) === false && el.barReviewed === true) {
                       counts.set(emailLowerCase, 1);
                   }
                   else {  }
               }
           );
           return counts;
        };

        const safeData = (Array.isArray(data) === false) ? [] : data;
        this.completionsPerEmail = countReviewCompletionsPerEmail(safeData);
    }

    private filterSubmissionsPerEmailByMinSubmissionsPredicate(submissionsPerEmail: Array<ISubmissionsPerEmail>) {
        return submissionsPerEmail.filter(
            (item: ISubmissionsPerEmail) => {
                return item.SubmissionsCount >= this.MinNumSubmissionsPredicate;
            }
        );
    }

    /** Should be used to populate <select> <options> in the UI */
    public BuildSubmissionsAndCompletions(submissionsPerEmail: Array<ISubmissionsPerEmail>): Array<ISubmissionsPerEmail> {
        const data = this.filterSubmissionsPerEmailByMinSubmissionsPredicate(submissionsPerEmail);
        const submissionsAndCompletions =
            data
                .sort(this.sortService.SortBy('Email'))
                .map(
                    (d) => {
                        const emailLowerCase = d.Email.toLowerCase();

                        return {
                            Email: emailLowerCase,
                            SubmissionsCount: d.SubmissionsCount,
                            CompletionsPerEmail: this.GetCompletionsPerEmails().get(emailLowerCase) ?? 0
                        };
                    }
                );
        return submissionsAndCompletions;
    }

    public SetSubmissionsPerEmail(data: Array<IAssessmentGrievance>) {
        const countSubmissionsPerEmail = (
            grievanceData: Array<IAssessmentGrievance>
        ): Array<ISubmissionsPerEmail> => {
            const counts: Map<string, number> = new Map();
            grievanceData.forEach(
                (el) => {
                    const repGroupInfo: {GroupName1: string, Email: string | null} = undefined;
                        // // @ts-ignore
                        // attorneyData.default.find(
                        //     (x) => x.GroupName1 === el.attorney_group
                        // );

                    const emailLowerCase = (repGroupInfo && repGroupInfo.Email)
                        ? repGroupInfo.Email.toLowerCase()
                        : el.email.toLowerCase();

                    if (counts.has(emailLowerCase) === true) {
                        counts.set(emailLowerCase, (counts.get(emailLowerCase) + 1));
                    }
                    else {
                        counts.set(emailLowerCase, 1);
                    }
                }
            );
            const output: Array<ISubmissionsPerEmail> = [];
            for (const [key, value] of counts.entries()) {
                output.push({ Email: key, SubmissionsCount: value, CompletionsPerEmail: undefined });
            }
            return output;
        };

        const safeData = (Array.isArray(data) === false) ? [] : data;
        this.submissionsPerEmail = countSubmissionsPerEmail(safeData);
        this.SubmissionsAndCompletions = this.BuildSubmissionsAndCompletions(this.submissionsPerEmail);
    }

    constructor(
        private readonly httpService: HttpService,
        private readonly httpPublicService: HttpPublicService,
        private readonly sortService: SortService
    ) {
        //
    }

    public ngOnInit(): void {
         this.httpPublicService.GetRepresentatives().subscribe(
            (data) => {
                this.attorneyData = data;
            },
            (err) => {
                console.error(err);
                window.alert('An error occurred. Rep data was not loaded.');
            }
        );
    }

    public onMinNumSubmissionsChanged() {
        this.submissionsPerEmailCopy = this.filterSubmissionsPerEmailByMinSubmissionsPredicate(
            this.submissionsPerEmail
        );
        this.SubmissionsAndCompletions = this.BuildSubmissionsAndCompletions(this.submissionsPerEmailCopy);
    }

    public HandleSelectChangeEvent(e) { this.SetSelectedRepGroups = this.RepGroupSelectControl.value; }

    /** TODO: Replace DOM stuff with nativeElement stuff */
    private selectAll(
        shouldSelectAll: boolean,
        selectId: string = 'TargetEmailsSelect'
    ) {
        let element: any;
        if (typeof selectId === 'string') {
            element = document.getElementById(selectId);
        }
        if (element.type === 'select-multiple') {
            for (const option of element.options) {
                option.selected = shouldSelectAll;
            }
        }
    }

    public SelectAllOptions(shouldSelectAll: boolean = true) {
        this.selectAll(shouldSelectAll);
        this.SetSelectedRepGroups =
            this.submissionsPerEmail
                .filter((x) => x.SubmissionsCount >= this.MinNumSubmissionsPredicate)
                .map((x) => x.Email);
    }

    public DeselectAllOptions(shouldSelectAll: boolean = false) {
        this.selectAll(shouldSelectAll);
        this.SetSelectedRepGroups = [];
    }

    public RefreshEmailJobQueueData() {
        this.httpService.RefreshJobQueueData(this.UserName, this.Password)
            .subscribe(
                (data: Array<IEmailJobQueueData>) => {
                    if (!data || data.length === 0) {
                        this.EmailJobQueueData = [];
                        return;
                    }
                    this.EmailJobQueueData = data;
                },
                (err) => {
                    window.alert('An error occurred. Please retry.');
                    console.error(err);
                }
            );

    }

    private testEmailJobPrerequisites(
        userName: string,
        password: string,
        selectedEmails: Array<string>
    ): ITestJobPreReqsResult {
        if (!userName || !password) {
            return {
                TestPassed: false,
                Message: 'It appears you\'re not authorized to perform this request. Please log in.'
            };
        }
        if (selectedEmails?.length < 1) {
            return {
                TestPassed: false,
                Message: 'You must select at least one submitter.'
            };
        }
        return { TestPassed: true, Message: '' };
    }

    public SubmitJobs() {
        const satisfied = this.testEmailJobPrerequisites(
            this.UserName,
            this.Password,
            this.GetSelectedRepGroups
        );
        if (satisfied.TestPassed === false) {
            window.alert(satisfied.Message);
            return;
        }
        const numEmailsSelected: number = this.GetSelectedRepGroups.length;
        const userConfirmedAction: boolean = window.confirm(
            `This action will generate dispositions for the ${numEmailsSelected} selected submitter(s). \n` +
            'Select \'OK\' to continue, or select \'Cancel\' to stop the action.'
        );
        if (userConfirmedAction === false) { return; }

        this.IsTriggeringJob = true;
        this.httpService.SubmitDispositionsJob(
            this.UserName,
            this.Password,
            this.GetSelectedRepGroups
        ).subscribe(
            () => {
                window.alert(
                    'Your job was submitted successfully. ' +
                    'The disposition will be generated shortly.'
                );
                this.IsTriggeringJob = false;
            },
            (err) => {
                this.IsTriggeringJob = false;
                if (err.status === 403) {
                    window.alert(Resources.Admin.NotAuthorizedMessage);
                    return;
                }
                console.error(err);
                window.alert(
                    'An error occurred while submitting the job. Please retry.'
                );
            }
        );
    }
}

interface ISubmissionsPerEmail {
    Email: string;
    SubmissionsCount: number;
    CompletionsPerEmail: number;
}

interface ITestJobPreReqsResult {
    Message: string;
    TestPassed: boolean;
}

interface IEmailJobQueueData {
    Email: string;
    MunicipalityEmailForCC: string;
    IsTest: boolean;
}
