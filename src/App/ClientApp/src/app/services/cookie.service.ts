
import { Injectable } from '@angular/core';
import { Constants } from '../types/enums/constants';

@Injectable({
    providedIn: 'root'
})
export class CookieService {

    private readonly cookieName: string = Constants.CookieCacheKeyName;

    constructor() { }

    public GetCookie(
        name: string = this.cookieName,
        cookies: string = document.cookie
    ): string {
        const value = `; ${cookies}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) {
            return parts.pop().split(';').shift();
        }
        return '';
    }

    /**
     * TODO: You can only have one cookie for this app's domain if you force deletion of cookies like this...
     */
    public InvalidateCookie(
        name: string = this.cookieName,
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
