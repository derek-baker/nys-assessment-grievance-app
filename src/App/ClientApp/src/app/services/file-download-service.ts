import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class FileDownloadService {

    constructor() { }

    public BuildCsv(rows: Array<any>, keysObj?: any | undefined): Blob {
        console.log(rows);

        let csvContent = '';
        const firstOrDefaultRow = rows.find(() => true);
        csvContent += keysObj
            ? (Object.keys(keysObj) + '\r\n')
            : (Object.keys(firstOrDefaultRow) + '\r\n');

        rows.forEach((rowObj) => {
            const values = Object.values(rowObj)
                .map(
                    (v) => (v)
                        // @ts-ignore
                        ? v.replaceAll(',', ' ')
                        : v
                );
            const row = values.join(',');
            // @ts-ignore
            csvContent += row.replaceAll('\r', '').replaceAll('\n', '') + '\r\n';
        });
        return new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
    }

    public DownloadCsv(csvContentBlob: Blob, fileName: string): void {
        const getDateString = () => {
            const date = new Date();
            const year = date.getFullYear();
            const month = `${date.getMonth() + 1}`.padStart(2, '0');
            const day = `${date.getDate()}`.padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        const uri = URL.createObjectURL(csvContentBlob);
        const link = document.createElement('a');
        link.setAttribute('href', uri);
        link.setAttribute('download', `${fileName}_${getDateString()}.csv`);
        document.body.appendChild(link);

        link.click();
        link.remove();
    }
}
