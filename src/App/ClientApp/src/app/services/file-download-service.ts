import { Injectable } from '@angular/core';

const buildBlob = (blobParts: Array<BlobPart>, options: any) => new Blob(blobParts, options);

@Injectable({
    providedIn: 'root'
})
export class FileDownloadService {

    constructor(private readonly buildBlobFunc = buildBlob) {}

    public BuildCsv(rows: Array<any>, keysObj?: any | undefined): Blob {
        let csvContent = '';
        const firstOrDefaultRow = rows.find(() => true);

        const buildHeaders = () => {
            csvContent += keysObj
                ? (Object.keys(keysObj) + '\r\n')
                : (Object.keys(firstOrDefaultRow) + '\r\n');
        };
        buildHeaders();

        const buildRows = () => {
            rows.forEach((rowObj) => {
                const values = Object.values(rowObj)
                    .map(
                        (v: string) => (v)
                            // @ts-ignore
                            ? v.replaceAll?.(',', ' ')
                            : v
                    );
                const row = values.join(',');

                // @ts-ignore
                csvContent += row.replaceAll?.('\r', '').replaceAll?.('\n', '') + '\r\n';
            });
        };
        buildRows();

        return this.buildBlobFunc([csvContent], { type: 'text/csv;charset=utf-8' });
    }

    public DownloadCsv(csvContentBlob: Blob, fileName: string, documentObj = window.document): void {
        const getDateString = () => {
            const date = new Date();
            const year = date.getFullYear();
            const month = `${date.getMonth() + 1}`.padStart(2, '0');
            const day = `${date.getDate()}`.padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        const uri = URL.createObjectURL?.(csvContentBlob);
        const link = documentObj.createElement('a');
        link.setAttribute('href', uri);
        link.setAttribute('download', `${fileName}_${getDateString()}.csv`);
        documentObj.body.appendChild(link);

        link.click();
        link.remove();
    }
}
