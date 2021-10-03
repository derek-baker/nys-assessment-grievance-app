import { Component, OnInit } from '@angular/core';
import { HttpService } from 'src/app/services/http.service';

@Component({
    selector: 'app-fake-post',
    templateUrl: './fake-post.component.html',
    styleUrls: ['./fake-post.component.css']
})
export class FakePostComponent implements OnInit {

    constructor(private readonly http: HttpService) { }

    public ngOnInit(): void {
        //
    }

    /**
     * Only here for demo/testing purposes.
     */
    public async TriggerFakeNysRp524Prefill() {
        // @ts-ignore
        document.getElementById('fakeNysRp524Data').submit();
    }

    public TriggerFakeNysRp525Prefill() {
        // @ts-ignore
        document.getElementById('fakeNysRp525Data').submit();
    }
}
