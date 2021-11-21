import { Injectable } from '@angular/core';
import Cookies from 'js-cookie';

export interface ICookiesLibrary {
    remove: (cookieName: string) => void;
    get: (cookieName: string) => string;
}

@Injectable({
    providedIn: 'root'
})
export class CookieService {

    public readonly CookieNames = Object.freeze({
        prefill: 'Rp524PrefillData',
        session: 'Session'
    });

    constructor(private readonly cookie: ICookiesLibrary = Cookies) { }

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

    public RemoveCookie(cookieName: string) {
        this.cookie.remove(cookieName);
    }

    public GetSessionCookie(): { UserId: string } {
        const sessionCookie = this.cookie.get(this.CookieNames.session);
        const sessionObj = sessionCookie ?
            JSON.parse(sessionCookie)
            : undefined;

        return sessionObj;
    }
}
