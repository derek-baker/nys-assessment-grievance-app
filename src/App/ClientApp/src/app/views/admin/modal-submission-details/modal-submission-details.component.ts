import { Component, OnInit, Input } from '@angular/core';
import { ISelectedGrievance } from './../../../types/ISelectedApplication';
import { HttpService } from './../../../services/http.service';
import { IGetFilesListResponse } from './../../../types/IGetFilesListResponse';
import { IAssessmentGrievance } from 'src/app/types/IAssessmentGrievance';
import { ILabelledvalue } from 'src/app/types/ILabelledValue';

@Component({
    selector: 'app-modal-submission-details',
    templateUrl: './modal-submission-details.component.html',
    styleUrls: ['./modal-submission-details.component.css']
})
export class ModalSubmissionFilesComponent implements OnInit {

    @Input()
    public readonly SelectedGrievance: ISelectedGrievance;

    @Input()
    public readonly IsArchiveDownloading: boolean;

    @Input()
    public readonly DownloadFilesAssociatedWithSelectedGrievance: () => void;

    @Input()
    public readonly GetSelectedGrievance: () => Array<IAssessmentGrievance> | undefined;

    public FileList: Array<IGetFilesListResponse> = [];

    public IsRefreshingFilesList: boolean = false;

    private grievanceData: IAssessmentGrievance;

    public get GrievanceData(): Array<ILabelledvalue> {
        const pairs: Array<ILabelledvalue> = [];
        for (const propName in this.grievanceData) {
            if (this.grievanceData.hasOwnProperty(propName)) {
                const propValue: string =
                    (this.grievanceData[propName])
                        ? this.grievanceData[propName].toString()
                        : '';

                const maxLen = 40;
                // TODO: Refactor and unit test
                const cleanPropValue =
                    (propValue.length < maxLen)
                        ? propValue.substr(0, maxLen)
                        : `${propValue.substr(0, maxLen)}...`;
                pairs.push(
                    {
                        Label: propName.toUpperCase(),
                        Value: cleanPropValue
                    }
                );
            }
        }
        return pairs;
    }

    constructor(
        private readonly http: HttpService
    ) {
        //
    }

    public ngOnInit(): void {
        //
    }

    public CopyGrievanceId() {
        const copyTextInput = document.getElementById('SubmissionIdForCopyButton');

        copyTextInput.style.display = 'block';
        // @ts-ignore
        // Select the text field
        copyTextInput.select();
        // @ts-ignore
        // For mobile devices
        copyTextInput.setSelectionRange(0, 99999);

        /* Copy the text inside the text field */
        document.execCommand('copy');

        copyTextInput.style.display = 'none';
    }

    public RefreshSubmissionFilesList() {
        this.IsRefreshingFilesList = true;
        this.http.GetGrievanceFiles(this.SelectedGrievance.Guid).subscribe(
            (fileList) => {
                this.FileList = fileList;
                this.IsRefreshingFilesList = false;
            },
            (err) => {
                console.error(err);
                window.alert('An error occurred while fetching the data. Please retry.');
                this.IsRefreshingFilesList = false;
            }
        );
        const data = this.GetSelectedGrievance();
        if (data?.length !== 1) {
            throw new Error('ERROR: unable to retrive selected data from grid.');
        }
        this.grievanceData = data[0];
    }

    public DeleteFile(fileIndex: number) {
        const deleteWarning = 'This action will delete the selected file.';
        if (window.confirm(deleteWarning) !== true) { return; }

        this.IsRefreshingFilesList = true;

        this.http.DeleteGrievanceFile(this.FileList[fileIndex].fullName).subscribe(
            (fileList) => {
                this.FileList = fileList;
                this.RefreshSubmissionFilesList();
            },
            (err) => {
                if (err.status === 403) {
                    window.alert('You are not authorized to perform this action.');
                }
                else {
                    console.error(err);
                    window.alert('An error occurred while fetching the data. Please retry.');
                }
                this.IsRefreshingFilesList = false;
            }
        );
    }
}
