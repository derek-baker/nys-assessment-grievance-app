import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpServiceBase } from './http.service.base';
import { UserSettings } from '../types/UserSettings';
import { IGetGrievancesMissingRP524 } from '../types/ApiResponses/IGetGrievancesMissingRP524';

@Injectable({
    providedIn: 'root'
})
export class HttpAdminService extends HttpServiceBase {
    constructor(
        private readonly http: HttpClient
    ) {
        super();
    }

    public FindGrievancesMissingRp524(
        endpoint: string = '/api/admin/GetGrievancesMissingRP524'
    ) {
        return this.http.get<Array<IGetGrievancesMissingRP524>>(endpoint);
    }

    public SetRepresentatives(
        userName,
        password,
        reps: Array<any>,
        endpoint = '/api/representatives/SetReps'
    ) {
        return this.http.post(
            endpoint,
            { userName, password, reps },
            { headers: this.headers }
        );
    }

    public SetUserSettings(
        userName,
        password,
        settings: UserSettings,
        endpoint = '/api/usersettings/setusersettings'
    ) {
        return this.http.post(
            endpoint,
            { userName, password, settings },
            { headers: this.headers }
        );
    }

    public GetGrievancesCsv(
        userName,
        password,
        endpoint: string = '/api/download/ExportGrievancesCsv'
    ) {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }
}
