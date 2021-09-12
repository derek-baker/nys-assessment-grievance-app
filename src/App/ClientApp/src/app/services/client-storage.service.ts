import { Injectable, Optional, Inject } from '@angular/core';
// import { RP524FormDataPreFill } from '../types/RP524FormData';
import { HAS_ACCEPTED_RP524_TERMS, SESSION_HASH_KEY } from '../tokens/client.storage.prefix.token';

@Injectable({
    providedIn: 'root'
})
export class ClientStorageService {

    // private readonly rp524DataKeyPrefix: string;
    private readonly hasAcceptedRp524TermsKey: string;
    private readonly sessionHashKey: string;

    constructor(
        // @Inject(RP524_STORAGE_PREFIX)
        // @Optional()
        // inProgressRp524DataKeyPrefix: string,

        @Inject(HAS_ACCEPTED_RP524_TERMS)
        @Optional()
        hasAcceptedTermsKey: string,

        @Inject(SESSION_HASH_KEY)
        @Optional()
        sessionHashKey: string
    ) {
        // this.rp524DataKeyPrefix = inProgressRp524DataKeyPrefix;
        this.hasAcceptedRp524TermsKey = hasAcceptedTermsKey;
        this.sessionHashKey = sessionHashKey;
    }

    // /**
    //  * Read keys for data from persistent store
    //  */
    // public GetDataKeysForSavedRp524s(
    //     storage = localStorage,
    //     keyPrefix = this.rp524DataKeyPrefix
    // ): Array<string> {
    //     const keys: Array<string> = [];
    //     for (const prop in storage) {
    //         if (prop.includes(keyPrefix)) {
    //             keys.push(prop);
    //         }
    //     }
    //     return keys;
    // }

    // /**
    //  * Write data to storage.
    //  */
    // public SetData(
    //     data: RP524FormDataPreFill,
    //     saveAs: string,
    //     storage = localStorage,
    //     prefix: string = this.rp524DataKeyPrefix
    // ): void {
    //     const key = `${prefix}${saveAs}`;
    //     storage.setItem(
    //         key,
    //         JSON.stringify(data)
    //     );
    // }

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
