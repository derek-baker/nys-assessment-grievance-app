import { Component, OnInit } from '@angular/core';
import { RP524FormDataPreFill } from 'src/app/types/RP524FormData';
import { HttpService } from 'src/app/services/http.service';

@Component({
    selector: 'app-fake-post',
    templateUrl: './fake-post.component.html',
    styleUrls: ['./fake-post.component.css']
})
export class FakePostComponent implements OnInit {

    // private readonly rp524PrefillData: RP524FormDataPreFill = {
    //     TwoCharAssessmentYear: 22,
    //     Muni: 'Somewhere',
    //     OwnerNameLine1: 'Some Person',
    //     OwnerNameLine2: 'Some Otherperson',
    //     OwnerAddressLine1: '222 Some St.',
    //     OwnerAddressLine2: 'Sometown, ZZ 12345',
    //     LocationStreetAddress: '222 Some St.',
    //     LocationCityTown: 'Sometown',
    //     LocationVillage: 'Somevillage',
    //     LocationCounty: 'Somecounty',
    //     SchoolDistrict: 'Some District',
    //     TaxMapNum: '1-2-3.4',
    //     ResidenceCheck: true,
    //     CommercialCheck: false,
    //     FarmCheck: false,
    //     IndustrialCheck: false,
    //     VacantCheck: false,
    //     OtherCheck: false,
    //     PropertyDescription: 'lorem',
    //     LandVal: 9980,
    //     TotalVal: 9990
    // };

    constructor(private readonly http: HttpService) { }

    public ngOnInit(): void {
        //
    }

    /**
     * Only here for demo purposes.
     * DOCS: https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch
     */
    public async TriggerFakeNysRp524Prefill() {
        // @ts-ignore
        // try {
        //     const res = await this.http.TriggerSyntheticRp524Prefill(this.rp524PrefillData);
        //     console.warn(res);
        //     if (res.redirected === true) {
        //         window.location.href = res.url;
        //     }
        // }
        // catch (err) {
        //     console.error(err);
        //     window.alert(err);
        // }
        // @ts-ignore
        document.getElementById('fakeNysRp524Data').submit();
    }

    public TriggerFakeNysRp525Prefill() {
        // @ts-ignore
        document.getElementById('fakeNysRp525Data').submit();
    }
}
