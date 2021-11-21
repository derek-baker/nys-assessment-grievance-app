import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class SortService {

    constructor() {}

    public BuildCompareFunc(
        field: string,
        isDescending: boolean = false,
        prepDataFunc = (value: string | number) => value.toString().toUpperCase()
    ) {
        const key = prepDataFunc
            ? (x: any) => prepDataFunc(x[field])
            : (x: any) => x[field];

        const isAscending = (!isDescending) ? 1 : -1;

        return (a: any, b: any) => {
            // @ts-ignore
            return a = key(a), b = key(b), isAscending * ((a > b) - (b > a));
        };
    }
}
