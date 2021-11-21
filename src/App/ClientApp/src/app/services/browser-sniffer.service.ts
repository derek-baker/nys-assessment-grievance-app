import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class BrowserSnifferService {

    public TestBrowserValidity(
        documentObj: Document = window.document,
        navigatorObj: any = window.navigator
    ) {
        // @ts-ignore
        const isBrowserInternetExplorer = () => documentObj.documentMode ? true : false;
        const isBrowserLegacyEdge = () => navigatorObj.userAgent.indexOf('Edge') > -1;

        return (isBrowserInternetExplorer() || isBrowserLegacyEdge())
            ? false
            : true;
    }
}
