import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class SortService {

    constructor() {
        //
    }

    /** TODO: Unit tests */
    public SortBy(
        field: string,
        descending: boolean = false,
        primer: (valueBeingSorted: number | string) => number | string = (value) => value.toString().toUpperCase()
    ) {
        const key = primer ?
            (x: any) => {
                return primer(x[field]);
            } :
            (x: any) => {
                return x[field];
            };

        const isAscending = (!descending) ? 1 : -1;

        return (a: any, b: any) => {
            // @ts-ignore
            return a = key(a), b = key(b), isAscending * ((a > b) - (b > a));
        };
    }
}
