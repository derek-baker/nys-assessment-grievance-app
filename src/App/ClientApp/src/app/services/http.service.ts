import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IGetFilesListResponse } from '../types/IGetFilesListResponse';
import { IAuthResponse } from '../types/ApiResponses/IAuthResponse';
import { IAssessmentGrievance } from '../types/IAssessmentGrievance';
import { HttpServiceBase } from './http.service.base';

@Injectable({
    providedIn: 'root'
})
export class HttpService extends HttpServiceBase {
    constructor(
        private readonly http: HttpClient
    ) {
        super();
    }

    public GetTimelineSetting(
        endpoint: string = '/api/UserSettings/GetUserSettings'
    ) {
        return this.http.get<any>(endpoint);
    }

    public CheckForPreviousSubmissionsForParcel(
        email: string,
        taxMapId: string,
        endpoint: string = '/api/CheckSubmission/PostCheckForPreviousSubmission'
    ): Observable<{ message: string }> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { email, taxMapId },
            { headers }
        );
    }

    public GenerateChangeReport(
        dateFilterStart: string,
        dateFilterEnd: string,
        endpoint: string = `/api/admin/GetChangeReport?start=${dateFilterStart}&end=${dateFilterEnd}`
    ): Observable<Array<any>> {
        return this.http.get<any>(endpoint);
    }

    public GetSubmissionData(
        endpoint: string = `/api/admin/GetGrievanceData`
    ): Observable<any> {
        return this.http.get<any>(endpoint);
    }

    public GetGrievanceJson(
        guid: string,
        endpoint: string = `/api/admin/GetGrievanceJson?guidString=${guid}`
    ): Observable<any> {
        return this.http.get<any>(endpoint);
    }

    public GetGrievanceFiles(
        guid: string,
        userName: string,
        password: string,
        endpoint: string = `/api/admin/GetGrievanceFiles`
    ): Observable<Array<IGetFilesListResponse>> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { userName, password, SubmissionGuid: guid },
            { headers }
        );
    }

    public DeleteGrievanceFile(
        blobFullName: string,
        userName: string,
        password: string,
        endpoint: string = `/api/admin/DeleteGrievanceFile`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { userName, password, blobFullName },
            { headers }
        );
    }

    public GetNysRp524WithFillableBoardOnly(
        endpoint: string = '/api/download'
    ) {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }

    public GetRemoteRp525PrefillData(
        userName: string,
        password: string,
        taxMapId: string,
        // TODO: This should come from IMO
        endpoint: string = '/api/PrefillRp525'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        const reqParams = {
            userName,
            password,
            taxMapId
        };
        return this.http.post(
            endpoint,
            reqParams,
            { headers }
        );
    }

    public ValidateGuid(
        guidString: string,
        endpoint: string = `/api/guid/TestGuidExistence/${guidString}`
    ): Observable<{isValid: boolean, taxMapId: string}> {
        return this.http.get<any>(endpoint);
    }

    public AuthUser(
        userName: string,
        password: string,
        endpoint: string = '/api/auth',
        headers = (new HttpHeaders()).set('Content-Type', 'application/json')
    ): Promise<IAuthResponse> {
        const userInfo = { userName, password };
        return this.http.post<any>(endpoint, userInfo, { headers }).toPromise();
    }

    public SetIsDownloadedStatus(
        guid: string,
        isReviewed: boolean,
        userName: string,
        reviewerPassword: string,
        endpoint: string = '/api/admin/PostDownloadedStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed, userName, password: reviewerPassword },
            { headers }
        );
    }

    public UpdateGrievance(
        userName: string,
        password: string,
        grievance: IAssessmentGrievance,
        endpoint: string = '/api/admin/PostEditGrievance'
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { userName, password, grievance },
            { headers }
        );
    }

    public GetOnlineBarReview(
        submissionId: string,
        endpoint: string = `/api/OnlineBarReview/GetBarReview?submissionGuid=${submissionId}`
    ) {
        return this.http.get<any>(endpoint);
    }

    public async SaveOnlineBarReview(
        data: any,
        endpoint: string = '/api/OnlineBarReview/PostBarReviewSave'
    ): Promise<Response> {
        const response = await fetch(
            endpoint,
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            }
        );
        return response;
    }

    public async UploadOnlineBarReviewData(
        data: any,
        endpoint: string = '/api/OnlineBarReview/PostBarReviewResult'
    ): Promise<Response> {
        const response = await fetch(
            endpoint,
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            }
        );
        return response;
    }

    public SetBarReviewStatus(
        guid: string,
        isBarReviewed: boolean,
        userName: string,
        reviewerPassword: string,
        endpoint: string = '/api/admin/PostBarReviewStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed: isBarReviewed, userName, password: reviewerPassword },
            { headers }
        );
    }

    public SetHearingCompletedStatus(
        guid: string,
        isHearingCompleted: boolean,
        userName: string,
        reviewerPassword: string,
        endpoint: string = '/api/admin/PostPersonalHearingStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed: isHearingCompleted, userName, password: reviewerPassword },
            { headers }
        );
    }

    /** TODO: Auth */
    public DownloadFilesForReview(
        guid: string,
        endpoint: string = `/api/download/${guid}`
    ) {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }

    public DownloadPrefilledRp525(
        userName: string,
        password: string,
        data: any,
        endpoint: string = `/api/admin/Post525PrefillData`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { userName, password, serializedData: JSON.stringify(data) },
            { headers, responseType: 'blob' }
        );
    }

    public SubmitDispositionsJob(
        userName: string,
        password: string,
        emailsToTarget: Array<string>,
        // muniEmailForCc: string,
        // isTest: boolean,
        endpoint: string = `/api/email/PostDispositionJobInput`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { userName, password, emailsToTarget}, // muniEmailForCc, isTest },
            { headers }
        );
    }

    public DeleteGrievanceSoftly(
        userName: string,
        password: string,
        grievanceId: string,
        endpoint: string = '/api/admin/PostGrievanceDeleteRequest'
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { userName, password, grievanceId },
            { headers }
        );
    }

    public SendApiKeyRequest(
        group: string,
        email: string,
        phone: string,
        address: string,
        site: string,
        uploadCountEstimate: number,
        endpoint: string = '/api/email/PostBulkUploadKeyRequest'
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            {
                Group: group,
                Email: email,
                Phone: phone,
                Address: address,
                Site: site,
                UploadCountEstimate: uploadCountEstimate
            },
            { headers }
        );
    }

    public RefreshJobQueueData(
        userName: string,
        password: string,
        endpoint: string = '/api/admin/ViewDispositionGenerationQueue',
        headers: HttpHeaders = (new HttpHeaders()).set('Content-Type', 'application/json')
    ): Observable<any> {
        return this.http.post(
            endpoint,
            { userName, password },
            { headers }
        );
    }
}
