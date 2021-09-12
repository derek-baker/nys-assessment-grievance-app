import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpServiceBase } from './http.service.base';
import { IAttorneyPrefillData } from '../types/IAttorneyPrefillData';
import { UserSettings } from '../types/UserSettings';

@Injectable({
    providedIn: 'root'
})
export class HttpPublicService extends HttpServiceBase {
    constructor(
        private readonly http: HttpClient
    ) {
        super();
    }

    public GetRepresentatives(
        endpoint = '/api/representatives/GetAllReps'
    ) {
        return this.http.get<Array<IAttorneyPrefillData>>(endpoint);
    }

    public GetUserSettings(
        endpoint = '/api/usersettings/getusersettings'
    ) {
        return this.http.get<UserSettings>(endpoint);
    }
}
