import { Component, Input, OnInit } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { HttpAdminService } from 'src/app/services/http.service.admin';
import { HttpPublicService } from 'src/app/services/http.service.public';
import { UserSettings } from 'src/app/types/UserSettings';

@Component({
    selector: 'app-admin-settings',
    templateUrl: './settings.component.html',
    styleUrls: ['./settings.component.css']
})
export class AdminSettingsComponent implements OnInit {

    public Settings: UserSettings = new UserSettings();

    constructor(
        private readonly httpPublic: HttpPublicService,
        private readonly httpAdmin: HttpAdminService,
        private readonly spinner: NgxSpinnerService
    ) { }

    public ngOnInit(): void {
        this.httpPublic.GetUserSettings().subscribe(
            (data) => {
                const parseDateString = (dateString) => new Date(dateString).toISOString().split('T')[0];

                data.submissionsStartDate = parseDateString(data.submissionsStartDate);
                data.submissionsEndDate = parseDateString(data.submissionsEndDate);
                data.supportingDocsEndDate = parseDateString(data.supportingDocsEndDate);

                this.Settings = data;
            },
            (error) => {
                window.alert('An error occurred');
                console.error(error);
            }
        );
    }

    public SaveSettings() {
        this.spinner.show();

        this.httpAdmin.SetUserSettings(this.Settings).subscribe(
            () => {
                this.spinner.hide();
                window.alert('Settings saved successfully');
            },
            (error) => {
                this.spinner.hide();
                window.alert('An error occurred');
                console.error(error);
            }
        );
    }

    public Clean() {

    }
}
