import { HttpHeaders } from '@angular/common/http';

export abstract class HttpServiceBase {
    protected readonly headers: HttpHeaders = (new HttpHeaders()).set('Content-Type', 'application/json');
}
