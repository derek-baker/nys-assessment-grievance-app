import { Component, OnInit, Input } from '@angular/core';
import { FileDownloadService } from 'src/app/services/file-download-service';
import { HttpPublicService } from 'src/app/services/http.service.public';
import { AttorneyPrefillDataHeader } from 'src/app/types/IAttorneyPrefillData';
import { parse } from 'papaparse';
import { HttpAdminService } from 'src/app/services/http.service.admin';

@Component({
    selector: 'app-modal-set-representatives',
    templateUrl: './modal-set-representatives.component.html',
    styleUrls: ['./modal-set-representatives.component.css']
})
export class ModalSetRepresentativesComponent implements OnInit {

    public IsMakingNetworkRequest: boolean = false;

    private uploadedReps: Array<any>;
    private readonly expectedRepDataLength = 13;

    constructor(
        private readonly httpPublic: HttpPublicService,
        private readonly httpAdmin: HttpAdminService,
        private readonly fileDownloadService: FileDownloadService
    ) { }

    public ngOnInit(): void {

    }

    public GetFile(event) {
        const file = event.target.files[0];
        const reader = new FileReader();
        reader.onload = (e) => {
            const csvContents = e.target.result;
            const result = parse<Array<any>>(csvContents as string);

            if (result.data.find(() => true)?.length !== this.expectedRepDataLength) {
                window.alert(`Your file appears malformed. Please download the current file using the 'Get Current Reps' button.`);
                return;
            }

            if (result.data.length < 2) {
                window.alert('Your file contains no rep data. Please upload a file containing rep data.');
                return;
            }

            this.uploadedReps = result.data;
        };
        reader.readAsText(file);
    }

    public GetCurrentReps() {
        this.httpPublic.GetRepresentatives().subscribe(
            (data) => {
                this.fileDownloadService.DownloadCsv(
                    this.fileDownloadService.BuildCsv(data, new AttorneyPrefillDataHeader()),
                    'CurrentReps.csv'
                );
            },
            (error) => {
                window.alert('An error occurred.');
                console.error(error);
                this.IsMakingNetworkRequest = false;
            }
        );
    }

    public SaveUploadedReps() {
        if (!this.uploadedReps || this.uploadedReps.length === 0) {
            window.alert('Please upload a CSV containing your rep data.');
            return;
        }

        const nonEmptyArrays = this.uploadedReps.filter((r) => r.length === this.expectedRepDataLength);
        const propNames = nonEmptyArrays[0];
        const repObjects = [];
        nonEmptyArrays.shift();

        for (const array of nonEmptyArrays) {
            const repObj = {};
            array.forEach((val, index) => {
                repObj[propNames[index]] = val;
            });
            repObjects.push(repObj);
        }

        this.httpAdmin.SetRepresentatives(repObjects).subscribe(
            () => {
                this.IsMakingNetworkRequest = false;

                document.getElementById('EditRepsInput')
                    // @ts-ignore
                    .value = '';

                window.alert('Uploaded reps were saved.');
            },
            (error) => {
                window.alert('An error occurred.');
                console.error(error);
                this.IsMakingNetworkRequest = false;
            }
        );
    }

    public Clean() {

    }
}
