import { Injectable, Optional, Inject } from '@angular/core';
import { HAS_ACCEPTED_RP524_TERMS } from '../tokens/client.storage.prefix.token';

@Injectable({
    providedIn: 'root'
})
export class ClientStorageService {

    private readonly hasAcceptedGrievanceTermsKey: string;

    constructor(
        @Inject(HAS_ACCEPTED_RP524_TERMS)
        @Optional()
        hasAcceptedTermsKey: string
    ) {
        this.hasAcceptedGrievanceTermsKey = hasAcceptedTermsKey;
    }

    public GetData(key: string, storage = localStorage): string {
        return storage.getItem(key);
    }
}
