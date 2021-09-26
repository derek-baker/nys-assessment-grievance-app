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
    public GetData(key: string, storage = localStorage): string {
        return storage.getItem(key);
    }
}
