import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'app-staged-files',
    templateUrl: './staged-files.component.html',
    styleUrls: ['./staged-files.component.css']
})
export class StagedFilesComponent implements OnInit {

    @Input()
    public readonly IdOfWatchedFileInput: string;

    public Files: Array<any> = [];

    constructor() {
        //
    }

    public ngOnInit(doc = document): void {
        const fileInput = doc.getElementById(this.IdOfWatchedFileInput);

        fileInput.addEventListener('change', () => {
            this.Files = Array.from(fileInput
                // @ts-ignore
                .files)
                .map((f: File) => f.name);
        });
    }

    public ClearFiles(doc = document) {
        const fileInput = doc.getElementById(this.IdOfWatchedFileInput);

        // @ts-ignore
        fileInput.value = '';
        this.Files = [];
    }
}
