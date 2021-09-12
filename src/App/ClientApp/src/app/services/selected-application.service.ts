import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { ISelectedGrievance } from '../types/ISelectedApplication';

@Injectable({
    providedIn: 'root'
})
export class SelectedGrievanceService {

    private selectedGrievance = new Subject<ISelectedGrievance>();

    public SelectedGrievance$ = this.selectedGrievance.asObservable();

    public SetSelectedApplication(selected: ISelectedGrievance) {
        this.selectedGrievance.next(selected);
    }
}
