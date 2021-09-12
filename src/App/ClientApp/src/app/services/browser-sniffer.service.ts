import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class BrowserSnifferService {

    public TestBrowserValidity(
        documentObj: Document = window.document,
        navigatorObj: any = window.navigator
    ) {
        if (
            // .documentMode prop only present in Internet Explorer
            // @ts-ignore
            documentObj.documentMode
            ||
            // This should only detect Legacy Edge (non-Chromium-based)
            navigatorObj.userAgent.indexOf('Edge') > -1
        ) {
            return false;
        }
        return true;
    }
}
