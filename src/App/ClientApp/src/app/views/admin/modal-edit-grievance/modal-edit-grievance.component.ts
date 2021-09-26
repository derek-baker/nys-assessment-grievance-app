import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';
import { AssessmentGrievance } from 'src/app/types/AssessmentGrievance';

@Component({
    selector: 'app-modal-edit-grievance',
    templateUrl: './modal-edit-grievance.component.html',
    styleUrls: ['./modal-edit-grievance.component.css']
})
export class ModalEditGrievanceComponent implements OnInit {

    @Input()
    public GrievanceWrapper: { Grievance: AssessmentGrievance };

    /** Should be used to emit an event that will trigger desired behavior */
    @Output()
    private RefreshGridEvent = new EventEmitter<string>();

    public IsEditing: boolean = false;

    constructor(private readonly http: HttpService) { }

    public ngOnInit(): void {
        //
    }

    public async EditGrievance() {
        this.IsEditing = true;
        this.http.UpdateGrievance(this.GrievanceWrapper.Grievance).subscribe(
            () => {
                this.IsEditing = false;
                this.RefreshGridEvent.emit(JSON.stringify(this.GrievanceWrapper.Grievance));
                window.alert(
                    'The grievance was edited successfully.'
                );
            },
            (err) => {
                this.IsEditing = false;
                console.error(err);
                window.alert('ERROR: Please retry.');
            }
        );
    }

}
