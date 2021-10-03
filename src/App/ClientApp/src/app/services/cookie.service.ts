import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class CookieService {

    public readonly CookieNames = Object.freeze({
        prefill: 'Rp524PrefillData',
        session: 'Session'
    });

    constructor() { }

    private getCookie(cookieKey: string, documentcookies: string) {
        const cookies = documentcookies.split('; ');
        return cookies?.find((row) => row.startsWith(cookieKey));
    }

    public GetCookie(
        cookieKey: string,
        cookiesString: string = document.cookie
    ): string | undefined {
        if (!cookiesString) { return undefined; }

        const cookie = this.getCookie(cookieKey, cookiesString);
        const cookieValue = cookie && cookie.includes('=') ? cookie.split('=')[1] : undefined;
        return cookieValue;
    }

    // public InvalidatePrefillCookie(
    //     expirationDays: number = 0,
    //     browserDocument: Document = document,
    //     sameSiteSetting: string = 'SameSite=Strict'
    // ): void {
    //     const expiryDate = new Date();
    //     expiryDate.setTime(expiryDate.getTime() + (expirationDays * 24 * 60 * 60 * 1000));
    //     const expires = 'expires=' + expiryDate.toUTCString();

    //     const cookieToInvalidate = this.getCookie(this.CookieNames.prefill, browserDocument.cookie);
    //     console.warn('cookieToInvalidate')
    //     console.log(cookieToInvalidate)

    //     // const cookieParts = cookieToInvalidate.split(';');
    //     const invalidatedCookie = `${this.CookieNames.prefill}=;${sameSiteSetting};${expires};path=/`;
    //     console.warn('invalidatedCookie')
    //     console.log(invalidatedCookie)

    //     // browserDocument.cookie = name + '=' + value + ';' + sameSiteSetting + ';' + expires + ';path=/';
    //     console.log(browserDocument.cookie)
    //     console.log(browserDocument.cookie.replace(cookieToInvalidate, invalidatedCookie))
    //     browserDocument.cookie = browserDocument.cookie.replace(cookieToInvalidate, invalidatedCookie);
    //     console.log(browserDocument.cookie)
    // }
}
