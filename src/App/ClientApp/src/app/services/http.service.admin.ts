import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpServiceBase } from './http.service.base';
import { UserSettings } from '../types/UserSettings';
import { IGetGrievancesMissingRP524 } from '../types/ApiResponses/IGetGrievancesMissingRP524';
import { User } from '../types/User';

@Injectable({
    providedIn: 'root'
})
export class HttpAdminService extends HttpServiceBase {
    constructor(
        private readonly http: HttpClient
    ) {
        super();
    }

    public FindGrievancesMissingRp524(endpoint: string = '/api/admin/GetGrievancesMissingRP524') {
        return this.http.get<Array<IGetGrievancesMissingRP524>>(endpoint);
    }

    public SetRepresentatives(reps: Array<any>, endpoint = '/api/representatives/SetReps') {
        return this.http.post(
            endpoint,
            { reps },
            { headers: this.headers }
        );
    }

    public SetUserSettings(settings: UserSettings, endpoint = '/api/usersettings/setusersettings') {
        return this.http.post(
            endpoint,
            { settings },
            { headers: this.headers }
        );
    }

    public GetGrievancesCsv(endpoint: string = '/api/download/ExportGrievancesCsv') {
        return this.http.get(
            endpoint,
            { responseType: 'blob' }
        );
    }

    public GetUsers(endpoint = '/api/users/getUsers') {
        return this.http.get<Array<User>>(endpoint);
    }

    public CreateUser(userSettings: ICreateUserInput, endpoint = '/api/users/createUser') {
        return this.http.post(
            endpoint,
            userSettings,
            { headers: this.headers });
    }

    public DeleteUser(userId: string, endpoint = `api/users/deleteUser?userId=${userId}`) {
        return this.http.delete(endpoint);
    }

    public ResetUserPassword(userId: string, endpoint = `api/users/resetUserPassword?userId=${userId}`) {
        return this.http.post(endpoint, {headers: this.headers});
    }
}

interface ICreateUserInput {
    userName: string;
}
