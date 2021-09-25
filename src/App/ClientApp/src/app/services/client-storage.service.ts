import { Injectable, Optional, Inject } from '@angular/core';
import { HAS_ACCEPTED_RP524_TERMS, SESSION_HASH_KEY } from '../tokens/client.storage.prefix.token';

@Injectable({
    providedIn: 'root'
})
export class ClientStorageService {

    private readonly hasAcceptedRp524TermsKey: string;
    private readonly sessionHashKey: string;

    constructor(

        @Inject(HAS_ACCEPTED_RP524_TERMS)
        @Optional()
        hasAcceptedTermsKey: string,

        @Inject(SESSION_HASH_KEY)
        @Optional()
        sessionHashKey: string
    ) {
        this.hasAcceptedRp524TermsKey = hasAcceptedTermsKey;
        this.sessionHashKey = sessionHashKey;
    }

    /**
     * Read data from store
     */
    public GetData(
        key: string,
        storage = localStorage
    ): string {
        return storage.getItem(key);
    }

    public GetTermsAccepted(
        storage = sessionStorage
    ): boolean {
        if (this.GetData(this.hasAcceptedRp524TermsKey, storage)) {
            return true;
        }
        return false;
    }

    public SetTermsAccepted(
        key: string = this.hasAcceptedRp524TermsKey,
        storage = sessionStorage
    ): void {
        storage.setItem(key, 'true');
    }

    /** NOTE: Not currently used */
    public SetSessionHash(
        sessionHash: string,
        key: string = this.sessionHashKey,
        storage = sessionStorage
    ): void {
        storage.setItem(key, sessionHash);
    }
    /** NOTE: Not currently used */
    public GetSessionHash(
        key: string = this.sessionHashKey,
        storage = sessionStorage
    ): string | undefined {
        return storage.getItem(key);
    }
}
