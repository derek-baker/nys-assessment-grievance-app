import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { ISelectedGrievance } from '../types/ISelectedApplication';

@Injectable({
    providedIn: 'root'
})
export class SelectedGrievanceObservableService {

    private selectedGrievance = new Subject<ISelectedGrievance>();

    public SelectedGrievance = this.selectedGrievance.asObservable();

    public PublishSelectedGrievance(selected: ISelectedGrievance) {
        this.selectedGrievance.next(selected);
    }
}
