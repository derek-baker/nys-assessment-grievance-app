import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class CookieService {

    public readonly CookieNames = Object.freeze({
        prefill: '_cacheKey',
        session: 'Session'
    })

    constructor() { }

    public GetCookie(
        cookieKey: string,
        cookiesString: string = document.cookie
    ): string {
        if (!cookiesString) return undefined;

        const cookies = cookiesString.split('; ');
        const sessionCookie = cookies?.find(row => row.startsWith(cookieKey));
        const sessionCookieValue = sessionCookie && sessionCookie.includes('=') ? sessionCookie.split('=')[1] : undefined;

        return sessionCookieValue;
    }

    /**
     * TODO: You can only have one cookie for this app's domain if you force deletion of cookies like this...
     */
    public InvalidateCookie(
        value: string = '',
        exdays: number = 0,
        browserDocument: Document = document,
        sameSiteSetting: string = 'SameSite=Strict'
    ): void {
        const d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        const expires = 'expires=' + d.toUTCString();

        browserDocument.cookie = name + '=' + value + ';' + sameSiteSetting + ';' + expires + ';path=/';
    }
}
