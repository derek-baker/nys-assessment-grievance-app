import { Component, OnInit, Input } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';

interface IChangeReport {
    TaxMapSbl: string;
    ComplainantPropertyLocation: string;
    Complainant: string;
    AttorneyRepGroup: string;
    ComplainantMailingAddress: string;
    CoOpUnitNum: string;
    HasSignature: string;
    TentativeAssessedValue: string;
    BarTotalAssessedValue: string;
    BarFullMarketValue: string;
    ComplaintType: string;
    BarReviewDate: string;
    GrievanceReason: string;
    GrievanceId: string;
}

@Component({
    selector: 'app-modal-export-review',
    templateUrl: './modal-export-review.component.html',
    styleUrls: ['./modal-export-review.component.css']
})
export class ModalExportReviewComponent implements OnInit {

    @Input()
    public readonly UserName: string;
    @Input()
    public readonly Password: string;

    public IsGeneratingChangeReport: boolean = false;
    public DateFilterStart: string;
    public DateFilterEnd: string;

    constructor(private readonly http: HttpService) { }

    public ngOnInit(): void {
        //
    }

    private validateReportParams(
        propsToValidate = [this.DateFilterStart, this.DateFilterEnd]
    ): boolean {
        const isUndefined = (date: string) => {
            return (date) ? false : true;
        };
        const isMalformed = (date: string) => {
            if (!date) { return false; }
            return date.split('-').length !== 3;
        };
        if (
            propsToValidate.some((x) => isUndefined(x) === true)
            ||
            propsToValidate.some((x) => isMalformed(x) === true)
        ) {
            return false;
        }
        return true;
    }

    // TO DO: refactor to server-side settings
    public GenerateChangeListReport(levelOfAssessment = .0209) {
        if (this.validateReportParams() === false) {
            window.alert('VALIDATION ERROR: You must supply both a start date filter and an end date value.');
            return;
        }

        this.IsGeneratingChangeReport = true;
        this.http.GenerateChangeReport(this.DateFilterStart, this.DateFilterEnd).subscribe(
            (results) => {
                if (!results || results.length === 0) {
                    window.alert('There were no reviews completed in the range you specified.');
                    return;
                }

                const data: Array<IChangeReport> = results.map(
                    (d): IChangeReport => {
                        const answers = JSON.parse(d.nys_rp525_answers);
                        const complainantPropertyLocation =
                            (answers && answers.Admin_Rp525_Location1 && answers.Admin_Rp525_Location2)
                                ? answers.Admin_Rp525_Location1 + ' ' + answers.Admin_Rp525_Location2
                                : d.complainant_mail_address;

                        return {
                            TaxMapSbl: d.tax_map_id,
                            ComplainantPropertyLocation: complainantPropertyLocation,
                            Complainant: (answers) ? answers.Admin_Rp525_ComplainantInfoTextArea : '',
                            AttorneyRepGroup: d.attorney_group,
                            ComplainantMailingAddress: d.complainant_mail_address,
                            CoOpUnitNum: d.co_op_unit_num,
                            HasSignature: ((answers?.SignatureAsBase64String) ? true : false).toString(),
                            TentativeAssessedValue: (d.nys_rp525_tentative) ? d.nys_rp525_tentative.replaceAll(',', '') : '',
                            BarTotalAssessedValue: (answers && answers.Admin_Rp525_Total) ? answers.Admin_Rp525_Total.replaceAll(',', '') : '',

                            // BAR Total Assessed Value divided by .0209 (Level of Assessment)
                            BarFullMarketValue:
                                (answers && answers.Admin_Rp525_Total)
                                    ? (
                                        Number(answers.Admin_Rp525_Total.replace('$', '').replaceAll(',', ''))
                                        / levelOfAssessment
                                    ).toString()
                                    : '',
                            GrievanceReason: d.reason,
                            ComplaintType: (d.complaint_type && d.complaint_type.length >= 3)
                                ? d.complaint_type.substring(2).trim()
                                : d.complaint_type,
                            BarReviewDate: d.barReviewDate,
                            GrievanceId: d.guid
                        };
                    }
                );

                this.downloadCsv(this.buildCsv(data));
            },
            (err) => {
                window.alert('An error occured. Please run the report for a shorter timespan.');
                this.IsGeneratingChangeReport = false;
            },
            () => {
                this.IsGeneratingChangeReport = false;
            }
        );
    }

    private buildCsv(rows: Array<IChangeReport>): Blob {
        let csvContent = '';
        const firstOrDefaultRow = rows.find(() => true);
        csvContent += (Object.keys(firstOrDefaultRow) + '\r\n');

        rows.forEach((rowObj) => {
            const values = Object.values(rowObj)
                .map(
                    (v) => (v) ? v.replaceAll(',', ' ') : v
                );
            const row = values.join(',');
            // @ts-ignore
            csvContent += row.replaceAll('\r', '').replaceAll('\n', '') + '\r\n';
        });
        return new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
    }

    private downloadCsv(csvContentBlob: Blob): void {
        const getDateString = () => {
            const date = new Date();
            const year = date.getFullYear();
            const month = `${date.getMonth() + 1}`.padStart(2, '0');
            const day = `${date.getDate()}`.padStart(2, '0');
            return `${year}-${month}-${day}`;
        };
        // const encodedUri = encodeURI(csvContent);
        const uri = URL.createObjectURL(csvContentBlob);
        const link = document.createElement('a');
        link.setAttribute('href', uri);
        link.setAttribute('download', `ChangeReport_${getDateString()}.csv`);
        document.body.appendChild(link);

        link.click();
        link.remove();
    }
}
