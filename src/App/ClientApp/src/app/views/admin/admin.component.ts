import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';
import { AgGridAngular } from 'ag-grid-angular';
import { RowSelectedEvent } from 'ag-grid-community';
import $ from 'jquery';
import { IRP525PrefillData } from 'src/app/types/IRP525PrefillData';
import { ISelectedGrievance } from 'src/app/types/ISelectedApplication';
import { SelectedGrievanceService } from 'src/app/services/selected-application.service';
import { IAssessmentGrievance } from 'src/app/types/IAssessmentGrievance';
import { ModalEmailDispositionsComponent } from './modal-generate-dispositions/modal-generate-dispositions.component';
import { ModalSubmissionFilesComponent } from './modal-submission-details/modal-submission-details.component';
import { IAuthResponse } from 'src/app/types/ApiResponses/IAuthResponse';
import { BrowserSnifferService } from 'src/app/services/browser-sniffer.service';
import { Router } from '@angular/router';
import { AssessmentGrievance } from 'src/app/types/AssessmentGrievance';
import { HttpAdminService } from 'src/app/services/http.service.admin';
import { FileDownloadService } from 'src/app/services/file-download-service';
import { HttpPublicService } from 'src/app/services/http.service.public';
import { CookieService } from 'src/app/services/cookie.service';
import { ISession } from 'src/app/types/ISession';

@Component({
    selector: 'app-admin',
    templateUrl: './admin.component.html',
    styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {

    private readonly settingsModalId = 'Admin_Settings';
    private readonly repsModalId: string = 'Admin_SetRepresentatives';
    private readonly editGrievanceModalId: string = 'Admin_EditGrievance';
    private readonly addFilesModalId: string = 'AddFilesToSubmission';
    private readonly createSubmissionModalId: string = 'CreateSubmissionModal';
    private readonly barReviewModalId: string = 'BarReviewModal';
    private readonly barReviewOnlineModalId: string = 'BarReviewOnlineModal';
    private readonly emailDispositionsModalId: string = 'Admin_EmailDispositions_Modal';
    private readonly listFilesModalId: string = 'Admin_ListFiles_Modal';
    private readonly exportReviewModalId: string = 'Admin_ExportReview_Modal';

    private readonly nothingSelectedMessage: string =
        'You don\'t have a grievance selected. \n' +
        'To select a grievance, click one of the checkboxes in the Tax Map ID column in the grid.';
    private readonly notAuthorizedMessage: string =
        'You are not authorized to perform this action. \n' +
        'Please request assistance from a user with the necessary authorizations.';

    /** Ag-Grid jacks into this via a directive to get its data. */
    public GrievanceStatuses: Array<IAssessmentGrievance> = [];

    public UserName: string = '';
    public Password: string = '';
    public UserAuthenticated: boolean = false;

    public IsFetchingData = false;
    public IsExportingAllGrievances = false;
    public IsArchiveDownloading = false;
    public AreAllArchivesDownloading = false;
    public IsValidatingPassword = false;
    public IsUploading = false;
    public IsDownloadingPrefilled525 = false;
    public IsFindingAppealsLacking524 = false;
    public IsFetchingReviewData = false;
    public IsDeletingGrievance = false;
    public IsLoadingDispositionsModal = false;
    public IsValidatingSession = false;
    public IsAppConfigured = true;

    public CanPrefillBarReview: boolean = true;

    public SelectedApplication: ISelectedGrievance = {
        Guid: '',
        TaxMapId: '',
        Email: '',
        EmailConfirm: '',
        MuniEmail: ''
    };

    /** Basically the same as the property above. Needed whole object for editing. */
    public SelectedGrievanceWrapper: { Grievance: IAssessmentGrievance } = {
        Grievance: new AssessmentGrievance()
    };

    /** */
    private rp524Data: any = {};

    /**
     * TODO: Refactor to shared data component
     * The field values need to kept in line with data in the DB/store
     */
    public readonly ColumnDefs: Array<{headerName: string, field: string, checkboxSelection?: boolean}> = [
        { headerName: 'Tax Map ID', field: 'tax_map_id', checkboxSelection: true },
        { headerName: 'Grievance ID', field: 'guid' },
        { headerName: 'Complainant', field: 'complainant' },
        { headerName: 'Submitter Email', field: 'email' },
        { headerName: 'Attorney Email', field: 'attorney_email' },
        { headerName: 'Submit Date', field: 'submit_date' },
        { headerName: 'Attorney/Rep Group', field: 'attorney_group' },
        { headerName: 'Attorney/Rep Phone', field: 'attorney_phone' },
        { headerName: 'Complainant Mailing Address', field: 'complainant_mail_address' },
        { headerName: 'Co-Op Unit Num', field: 'co_op_unit_num' },
        { headerName: 'Reason', field: 'reason' },
        { headerName: 'Notes', field: 'notes' },

        { headerName: 'Hearing Requested', field: 'requested_personal_hearing' },
        { headerName: 'Hearing Completed', field: 'completed_personal_hearing' },
        { headerName: 'Complaint Type', field: 'complaint_type' },
        { headerName: 'Proposed Value', field: 'proposed_value' },
        { headerName: 'Created By', field: 'creator_name' },

        { headerName: 'Bar Review Status', field: 'barReviewed' },
        { headerName: 'Bar Review Date', field: 'barReviewDate' },
        { headerName: 'Downloaded', field: 'downloaded' },
        { headerName: 'Download Date', field: 'download_date' },

        { headerName: 'Conflict of Interest', field: 'includes_conflict' },
        { headerName: 'Res Questionnaire', field: 'includes_res_questionnaire' },
        { headerName: 'Com Questionnaire', field: 'includes_com_questionnaire' },
        { headerName: 'Includes Auth Letter', field: 'includes_letter_of_auth' },

        { headerName: 'Includes Income Expense Forms', field: 'includes_income_expense_forms' },
        { headerName: 'Includes Income Expense Exclusion', field: 'includes_income_expense_exclusion' },
        { headerName: 'Includes Supporting Docs', field: 'includes_supporting_docs' },

        { headerName: 'Signature Type', field: 'five_signature_type' },

        { headerName: 'RP-525 Tentative', field: 'nys_rp525_tentative' },
        { headerName: 'RP-525 Is Reduced', field: 'nys_rp525_is_reduced' },
        { headerName: 'RP-525 Reduced To', field: 'nys_rp525_is_reduced_value' },
        { headerName: 'RP-525 All Answers', field: 'nys_rp525_answers' }
    ];
    public readonly DefaultColumnDef = {
        sortable: true,
        filter: true,
        resizable: true,
        enableCellChangeFlash: true
    };

    @ViewChild('agGrid')
    private readonly agGrid: AgGridAngular;

    @ViewChild(ModalEmailDispositionsComponent)
    private readonly emailDispositionComponent: ModalEmailDispositionsComponent;

    @ViewChild(ModalSubmissionFilesComponent)
    private readonly submissionFilesComponent: ModalSubmissionFilesComponent;

    constructor(
        private readonly httpService: HttpService,
        private readonly httpPublic: HttpPublicService,
        private readonly httpAdminService: HttpAdminService,
        private readonly selectedAppService: SelectedGrievanceService,
        private readonly browserSniffer: BrowserSnifferService,
        private readonly fileDownloadService: FileDownloadService,
        private readonly router: Router,
        private readonly cookieService: CookieService
    ) {}

    public LogOut(location = window.location) {
        this.cookieService.RemoveCookie(this.cookieService.CookieNames.session);
        location.reload();
    }

    /** Intended to auto-switch tabs when user highlights a row */
    public onRowSelected(event: RowSelectedEvent) {
        // For some reason, this tends to lag and results in the previously selected guid being cached
        // this.selectedRowGuid = event.node.data.guid;

        const tabToClick = document.getElementById('IndividualGrievanceTab');
        if (!tabToClick) {
            throw new Error('tabToClick is undefined');
        }
        if (!this.GetSelectedData()) {
            return;
        }
        tabToClick.click();
    }

    /**
     * TODO: This should only be called when the grid needs to be reloaded.
     *       (As opposed to needing a refresh)
     */
    public reloadGridEventHandler(): void {
        this.PopulateGridWithData();
    }

    /** WARNING: This assumes only one row can be selected */
    public RefreshRowEventHandler(data: string): void {
        const newData = JSON.parse(data);
        const oldData = this.GetSelectedData()[0];
        const rowUpdaterFunc = () => {
            // tslint:disable-next-line: forin
            for (const prop in oldData) {
                oldData[prop] = newData[prop];
            }
        };
        this.refreshGridData(rowUpdaterFunc);
    }

    public ngOnInit(): void {
        const sessionEncoded = this.cookieService.GetCookie(this.cookieService.CookieNames.session);
        if (sessionEncoded) {
            this.IsValidatingSession = true;
            const session: ISession = JSON.parse(decodeURIComponent(sessionEncoded));
            this.httpPublic.ValidateSession(session).subscribe(
                (result) => {
                    if (result.isValidSession === true) {
                        this.UserAuthenticated = true;
                        this.UserName = result.userName;
                    }
                },
                (err) => {
                    console.error(err);
                    window.alert('An error occurred.');
                },
                () => {
                    this.IsValidatingSession = false;
                }
            );
        }

        if (this.browserSniffer.TestBrowserValidity() === false) {
            this.router.navigate(['/warning']);
        }

        this.httpPublic.GetUserSettings().subscribe(
            (settings) => {
                if (!settings) {
                    this.IsAppConfigured = false;
                    return;
                }
                for (const propName in settings) {
                    if (!settings[propName]) {
                        this.IsAppConfigured = false;
                        return;
                    }
                }
            },
            (error) => {
                window.alert('An error occurred. Please report this issue.');
                console.error(error);
            }
        );

        this.IsFetchingData = true;
        this.httpService.GetSubmissionData()
            .subscribe(
                (result: Array<IAssessmentGrievance>) => {
                    const data = result;
                    // If the list returned from the API contains one element,
                    // it's being unwrapped into a string for some reason.
                    if (data === null || Array.isArray(data) === false) {
                        console.log('Wrapping data with array to handle edge case...');
                        this.GrievanceStatuses = [];
                        return;
                    }
                    this.GrievanceStatuses = data;

                    this.emailDispositionComponent.SetSubmissionsPerEmail(data);
                    this.emailDispositionComponent.SetCompletionsPerEmail = data;
                    this.IsFetchingData = false;
                },
                (error) => {
                    window.alert('An error occurred while loading data. Please refresh this page.');
                    console.error(error);
                    this.IsFetchingData = false;
                },
                () => {
                    this.IsFetchingData = false;
                }
            );

        this.UserName = this.cookieService.GetSessionCookie()?.UserId;
    }

    /** TODO: Refactor to use less DOM */
    public DisplayOptions(event: Event, optionsToDisplay: string) {
        let tabcontent: any;
        let tablinks: any;

        tabcontent = document.getElementsByClassName('tabcontent');
        for (const content of tabcontent) {
            content.style.display = 'none';
        }
        tablinks = document.getElementsByClassName('tablinks');
        for (const link of tablinks) {
            link.className = link.className.replace(' active', '');
        }
        document.getElementById(optionsToDisplay).style.display = 'block';
        // @ts-ignore
        event.currentTarget.className += ' active';
    }

    public PopulateGridWithData() {
        this.IsFetchingData = true;

        this.httpService.GetSubmissionData()
            .subscribe(
                (result: Array<IAssessmentGrievance>) => {
                    const data = result;
                    // If the list returned from the API contains one element,
                    // it's being unwrapped into a string for some reason.
                    if (data === null || Array.isArray(data) === false) {
                        console.log('Wrapping data with array to handle edge case...');
                        this.GrievanceStatuses = [];
                        return;
                    }
                    // This blows away filters and selections in the grid
                    this.GrievanceStatuses = data;

                    this.emailDispositionComponent.SetSubmissionsPerEmail(data);
                    this.IsFetchingData = false;
                },
                (error) => {
                    window.alert('An error occurred while loading data. Please refresh this page.');
                    console.error(error);
                    this.IsFetchingData = false;
                }
            );
    }

    // /** https://www.ag-grid.com/javascript-grid-row-node/#application-assigned-ids */
    // private GetRowNodeId(data: any) {
    //     return data.guid;
    // }

    /** WARNING: Method assumes multi-row selections are not enabled */
    public refreshGridData(dataUpdaterFunc: (rowToUpdate: any) => void) {
        this.IsFetchingData = true;

        this.httpService.GetSubmissionData().subscribe(
            (result: Array<IAssessmentGrievance>) => {
                const data = result;
                // If the list returned from the API contains one element,
                // it's being unwrapped into a string for some reason.
                if (data === null || Array.isArray(data) === false) {
                    this.GrievanceStatuses = [];
                    return;
                }
                const rowToUpdate = this.agGrid.api.getSelectedRows()[0];
                dataUpdaterFunc(rowToUpdate);
                this.agGrid.api.applyTransaction({ update: [rowToUpdate] });

                this.emailDispositionComponent.SetSubmissionsPerEmail(data);
                this.emailDispositionComponent.SetCompletionsPerEmail = data;
            },
            (error) => {
                window.alert('An error occurred while loading data. Please refresh this page.');
                console.error(error);
            },
            () => {
                this.IsFetchingData = false;
            }
        );
    }

    public async Authenticate(alertFunc = window.alert) {
        try {
            const userName: string = this.UserName.trim().toLowerCase();
            const password: string = this.Password.trim();
            if (!password || password.length === 0) {
                return alertFunc('Please input a password');
            }
            this.IsValidatingPassword = true;
            const authResult: IAuthResponse = await this.httpService.AuthUser(
                userName,
                password
            );

            if (authResult.authResult.isAuthenticated === true) {
                this.UserAuthenticated = true;
                return;
            }
            window.alert('Invalid password. Please try again.');
        }
        catch (error) {
            window.alert(
                'An error occurred while attempting to authenticate. Please check your internet connection, and retry.'
            );
            console.error(error);
        }
        finally {
            this.IsValidatingPassword = false;
        }
    }

    /** TODO: Stop returning an array with one element */
    public GetSelectedData(): Array<IAssessmentGrievance> | undefined {
        if (!this.agGrid || !this.agGrid.api) {
            return undefined;
        }
        const selectedNodes = this.agGrid.api.getSelectedNodes();
        const selectedData = selectedNodes.map((node) => node.data);
        if (selectedData.length === 0) {
            return undefined;
        }
        return selectedData;
    }

    /**
     * TODO: Add interface for data for strong typing
     */
    public DownloadPrefilledRp525ForOffline(data: any = this.rp524Data) {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        const taxMapId = selectedData[0].tax_map_id;
        this.IsDownloadingPrefilled525 = true;
        this.httpService.DownloadPrefilledRp525(data).subscribe(
            (response) => {
                try {
                    // TODO: Refactor to method
                    const link = document.createElement('a');
                    link.setAttribute('style', 'display: none');
                    link.href = window.URL.createObjectURL(response);
                    const fileName = this.removeIllegalCharsFromFilename(`NYS_RP525_${taxMapId}.pdf`);
                    link.download = fileName;
                    link.click();
                    link.remove();
                }
                catch (error) {
                    window.alert(
                        'Unable to download file. Please retry. See console for more info.'
                    );
                    console.error(error);
                }
            },
            (error: any) => {
                window.alert('An error occurred. See console for more info.');
                console.error(error);
                this.IsDownloadingPrefilled525 = false;
            },
            () => {
                this.IsDownloadingPrefilled525 = false;
            }
        );
    }

    public BeginGenerateChangeListReport(modalId: string = this.exportReviewModalId): void {
        $(`#${modalId}`).modal();
    }

    public BeginAddFilesToSubmission(
        modalId: string = this.addFilesModalId
    ) {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        this.SelectedApplication.Guid = selectedData[0].guid;
        this.SelectedApplication.TaxMapId = selectedData[0].tax_map_id;
        this.SelectedApplication.Email = selectedData[0].email;

        $(`#${modalId}`).modal();
    }

    public BeginEditGrievance(
        modalId: string = this.editGrievanceModalId
    ) {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        this.SelectedGrievanceWrapper.Grievance = selectedData[0];

        $(`#${modalId}`).modal();
    }

    public BeginDisplayOfSubmissionFiles(
        modalId: string = this.listFilesModalId
    ) {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        this.SelectedApplication.Guid = selectedData[0].guid;
        this.SelectedApplication.TaxMapId = selectedData[0].tax_map_id;
        this.SelectedApplication.Email = selectedData[0].email;

        this.submissionFilesComponent.RefreshSubmissionFilesList();

        $(`#${modalId}`).modal();
    }

    public BeginSubmissionCreation(
        modalId: string = this.createSubmissionModalId
    ) {
        $(`#${modalId}`).modal();
    }

    public BeginOnlineBarReview(): void {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        if (selectedData[0].barReviewed === true) {
            window.alert(
                'This grievance appears to have been reviewed previously. ' +
                'If you submit another review, it will overwrite the previous RP-525 file.'
            );
        }

        // INTENT: Clean up from any previous reviews that may have occurred
        this.SelectedApplication.Email = '';
        this.SelectedApplication.EmailConfirm = '';
        this.SelectedApplication.Guid = '';
        this.SelectedApplication.TaxMapId = '';

        const submissionId = selectedData[0].guid;
        const taxMapId = selectedData[0].tax_map_id;
        const email = selectedData[0].email;

        this.SelectedApplication.Guid = submissionId;
        this.SelectedApplication.TaxMapId = taxMapId;
        this.SelectedApplication.Email = email;
        this.SelectedApplication.EmailConfirm = email;

        this.selectedAppService.SetSelectedApplication(this.SelectedApplication);
        $(`#${this.barReviewOnlineModalId}`).modal();
    }

    /**
     * We needed to retrieve from storage the data the user submitted along with the RP-524
     */
    public BeginOfflineBarReview(): void {
        // INTENT: Clean up from any previous reviews that may have occurred
        this.SelectedApplication.Email = '';
        this.SelectedApplication.EmailConfirm = '';
        this.SelectedApplication.Guid = '';
        this.SelectedApplication.TaxMapId = '';

        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        this.IsFetchingReviewData = true;
        const submissionId = selectedData[0].guid;
        const taxMapId = selectedData[0].tax_map_id;
        const email = selectedData[0].email;

        this.httpService.GetGrievanceJson(
            submissionId
        ).subscribe(
            (data: IRP525PrefillData) => {
                this.SelectedApplication.Guid = submissionId;
                this.SelectedApplication.TaxMapId = taxMapId;
                this.SelectedApplication.Email = email;
                this.SelectedApplication.EmailConfirm = email;

                $(`#${this.barReviewModalId}`).modal();

                if (!data) {
                    this.CanPrefillBarReview = false;
                    window.alert(
                        'Unable to prefill NYS RP-525 for this application. \n' +
                        'Please manually complete NYS RP-525.'
                    );
                }
                else {
                    this.rp524Data = data;
                    this.CanPrefillBarReview = true;
                }
                this.IsFetchingReviewData = false;
            },
            (error) => {
                window.alert('An error occurred.');
                console.error(error);
                this.IsFetchingReviewData = false;
            }
        );
    }

    public OpenEditRepsModal(modalId = this.repsModalId) {
        $(`#${modalId}`).modal();
    }

    public OpenEditSettingsModal(modalId = this.settingsModalId) {
        $(`#${modalId}`).modal();
    }

    public BeginGenerateDispositions(
        modalId: string = this.emailDispositionsModalId,
        disabled = false
    ) {
        if (disabled === true) {
            window.alert('This feature is currently undergoing maintenance.');
            return;
        }
        this.IsLoadingDispositionsModal = true;
        this.httpService.GetSubmissionData()
            .subscribe(
                (result: Array<IAssessmentGrievance>) => {
                    const data = result;
                    // If the list returned from the API contains one element,
                    // it's being unwrapped into a string for some reason.
                    if (data === null || Array.isArray(data) === false) {
                        this.GrievanceStatuses = [];
                        return;
                    }
                    this.emailDispositionComponent.SetSubmissionsPerEmail(data);
                    this.emailDispositionComponent.SetCompletionsPerEmail = data;

                    this.IsFetchingData = false;
                    $(`#${modalId}`).modal();
                },
                (error) => {
                    window.alert('An error occurred while loading data. Please refresh this page.');
                    console.error(error);
                    this.IsFetchingData = false;
                },
                () => {
                    this.IsLoadingDispositionsModal = false;
                }
            );
    }

    /** TODO: Refactor to elsewhere and unit test */
    private removeIllegalCharsFromFilename(filename: string) {
        return filename.replace(/[/\\?%*:|"<>]/g, '_');
    }

    public DownloadFilesAssociatedWithSelectedGrievance(): void {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }
        this.IsArchiveDownloading = true;
        this.httpService.DownloadFilesForReview(selectedData[0].guid)
            .subscribe(
                (result) => {
                    try {
                        // TODO: Refactor to method
                        const link = document.createElement('a');
                        link.setAttribute('style', 'display: none');
                        link.href = window.URL.createObjectURL(result);
                        const fileName = this.removeIllegalCharsFromFilename(`${selectedData[0].tax_map_id}.zip`);
                        link.download = fileName;
                        link.click();
                        link.remove();

                        this.httpService.SetIsDownloadedStatus(selectedData[0].guid, true).subscribe(
                            () => {
                                const dataUpdater = (rowToUpdate: IAssessmentGrievance) => {
                                    rowToUpdate.downloaded = true;
                                    rowToUpdate.download_date = new Date().toLocaleDateString('en-us');
                                };
                                this.refreshGridData(dataUpdater);
                            },
                            (error) => {
                                window.alert('Unable to update data. See console for more info.');
                                console.error(error);
                            }
                        );
                    }
                    catch (error) {
                        window.alert('Unable to download files. Please retry. See console for more info.');
                        console.error(error);
                    }
                    finally {
                        this.IsArchiveDownloading = false;
                    }
                },
                (error) => {
                    window.alert('Unable to download files. Please retry. See console for more info.');
                    console.error(error);
                },
                () => {
                    this.IsArchiveDownloading = false;
                }
            );
    }

    public DeleteGrievance() {
        const selectedData = this.GetSelectedData();
        if (!selectedData) {
            window.alert(this.nothingSelectedMessage);
            return;
        }

        const confirmMessage =
            'This action will mark the selected grievance as deleted.';
        if (window.confirm(confirmMessage) === false) {
            return;
        }

        this.IsDeletingGrievance = true;

        this.httpService.DeleteGrievanceSoftly(selectedData[0].guid).subscribe(
            () => {
                this.IsDeletingGrievance = false;
                this.PopulateGridWithData();
            },
            (err) => {
                this.IsDeletingGrievance = false;
                if (err.status === 403) {
                    window.alert(this.notAuthorizedMessage);
                    return;
                }
                console.error(err);
                window.alert('ERROR: Please retry');
            }
        );
    }

    public ExportAllGrievancesToCsv(filename = `AllGrievances`): void {
        this.IsExportingAllGrievances = true;
        this.httpAdminService.GetGrievancesCsv().subscribe(
            (data) => {
                this.fileDownloadService.DownloadCsv(data, filename);
            },
            (error) => {
                console.error(error);
                window.alert('An error occurred.');
            },
            () => {
                this.IsExportingAllGrievances = false;
            }
        );
    }

    public FindGrievancesMissingRp524(filename = 'AppealsLackingRp524'): void {
        this.IsFindingAppealsLacking524 = true;

        this.httpAdminService.FindGrievancesMissingRp524().subscribe(
            (data) => {
                if (!data || !data.some(() => true)) {
                    window.alert('There are no grievances lacking RP-524 files.');
                    return;
                }

                this.IsFindingAppealsLacking524 = false;
                const dataBlob = this.fileDownloadService.BuildCsv(data);
                this.fileDownloadService.DownloadCsv(dataBlob, filename);
            },
            (error) => {
                window.alert('An error occurred. Please retry.');
                console.error(error);
            },
            () => {
                this.IsFindingAppealsLacking524 = false;
            }
        );
    }
}
