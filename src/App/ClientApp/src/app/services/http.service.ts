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
        endpoint: string = `/api/admin/GetGrievanceFiles`
    ): Observable<Array<IGetFilesListResponse>> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { SubmissionGuid: guid },
            { headers }
        );
    }

    public DeleteGrievanceFile(
        blobFullName: string,
        endpoint: string = `/api/admin/DeleteGrievanceFile`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { blobFullName },
            { headers }
        );
    }

    public GetNysRp524WithFillableBoardOnly(
        endpoint: string = '/api/download/getRp524Pdf'
    ) {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }

    public GetRemoteRp525PrefillData(
        taxMapId: string,
        endpoint: string = '/api/PrefillRp525'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        const reqParams = {
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
        endpoint: string = '/api/admin/PostDownloadedStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed },
            { headers }
        );
    }

    public UpdateGrievance(
        grievance: IAssessmentGrievance,
        endpoint: string = '/api/admin/PostEditGrievance'
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { grievance },
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
        endpoint: string = '/api/admin/PostBarReviewStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed: isBarReviewed },
            { headers }
        );
    }

    public SetHearingCompletedStatus(
        guid: string,
        isHearingCompleted: boolean,
        endpoint: string = '/api/admin/PostPersonalHearingStatus'
    ): Observable<any> {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post<any>(
            endpoint,
            { guid, isReviewed: isHearingCompleted },
            { headers }
        );
    }

    /** TODO: Auth */
    public DownloadFilesForReview(
        guid: string,
        endpoint: string = `/api/download/getGrievanceFiles?id=${guid}`
    ) {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }

    public DownloadPrefilledRp525(
        data: any,
        endpoint: string = `/api/admin/Post525PrefillData`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { serializedData: JSON.stringify(data) },
            { headers, responseType: 'blob' }
        );
    }

    public SubmitDispositionsJob(
        emailsToTarget: Array<string>,
        endpoint: string = `/api/email/PostDispositionJobInput`
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { emailsToTarget},
            { headers }
        );
    }

    public DeleteGrievanceSoftly(
        grievanceId: string,
        endpoint: string = '/api/admin/PostGrievanceDeleteRequest'
    ) {
        const headers = (new HttpHeaders()).set('Content-Type', 'application/json');
        return this.http.post(
            endpoint,
            { grievanceId },
            { headers }
        );
    }

    public RefreshJobQueueData(
        endpoint: string = '/api/admin/ViewDispositionGenerationQueue',
        headers: HttpHeaders = (new HttpHeaders()).set('Content-Type', 'application/json')
    ): Observable<any> {
        return this.http.post(
            endpoint,
            { },
            { headers }
        );
    }
}
