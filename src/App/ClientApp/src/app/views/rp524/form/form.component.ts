import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CookieService } from 'src/app/services/cookie.service';
import { RP524FormDataPreFill } from 'src/app/types/RP524FormData';
import { FormDataService } from 'src/app/services/form-data.service';
import { ClientStorageService } from 'src/app/services/client-storage.service';
import { IComponentCanDeactivate } from 'src/app/services/router-guard.service';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-form',
    templateUrl: './form.component.html',
    styleUrls: ['./form.component.css']
})
export class FormComponent implements OnInit, IComponentCanDeactivate {

    private data: RP524FormDataPreFill | any;

    /**
     * Initialized in constructor, but populated in child components.
     */
    public readonly ParentForm: FormGroup;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly formDataService: FormDataService,
        private readonly cookieService: CookieService,
        private readonly activatedroute: ActivatedRoute,
        private readonly clientStorage: ClientStorageService,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        this.ParentForm = this.formBuilder.group({});
        this.formDataService.InitFormRef(this.ParentForm);
        this.formDataService.UserWantsToSubmit = false;
    }

    public ngOnInit() {
        // this.ParentForm.valueChanges.subscribe(
        //     (newVal: any) => {
        //         // console.log(newVal);
        //     }
        // );

        // const cookieValue = decodeURIComponent(this.cookieService.GetCookie());
        // // The presence of the cookie should indicate that we're pre-filling a form (TODO: Use response body?)
        // if (cookieValue.length > 0) {
        //     try {
        //         const data = JSON.parse(cookieValue);
        //         // We set this value in anticipation of an event emitted by the last child component of this.
        //         this.data = data;
        //         // TODO: If we don't purge the cookie, will it re-prefill?
        //         this.cookieService.InvalidateCookie();
        //     }
        //     catch (err) {
        //         console.error(err);
        //         window.alert('Unable to prefill form data.');
        //     }
        //     return;
        // }

        // this.activatedroute.queryParams.subscribe(
        //     (params) => {
        //         // tslint:disable-next-line: no-string-literal
        //         const key = params['key'];
        //         if (key) {
        //             const data = JSON.parse(this.clientStorage.GetData(key));
        //             // NOTE: This is kind of sketchy. If all the child components render before the line below,
        //             //       the form values will be set by the event handler with incomplete or no data.
        //             this.data = data;
        //         }
        //     }
        // );
    }

    // @HostListener allows us to also guard against browser refresh, close, etc.
    @HostListener('window:beforeunload')
    public canDeactivate(): Observable<boolean> | boolean {
        if (this.ParentForm.dirty && this.formDataService.UserWantsToSubmit === false) {
            return false;
        }
        return true;
    }

    public onFormLoaded() {
        this.formDataService.SetFormValues(this.data);

        const watchedInput = document.getElementById('MarketValueEstimate');

        watchedInput.oninput = () => {
            this.ParentForm.controls.three_three_text.setValue(this.ParentForm.controls.MarketValueEstimate.value);
            this.ParentForm.controls.three_b_1_b_text.setValue(this.ParentForm.controls.MarketValueEstimate.value);
        };
        // Manually trigger event that triggers currency currency formatting
        // (so that POSTed data gets formatted)
        const event = new Event('input', { bubbles: true, cancelable: true });
        Array.from(document.getElementsByTagName('app-text-input'))
            .forEach(
                (el) => el.querySelector('input').dispatchEvent(event)
            );

        this.ParentForm.controls.three_b_1_a_text.setValue(this.ParentForm.controls.TotalVal.value);
        this.ParentForm.controls.four_three_text.setValue(this.ParentForm.controls.Muni.value);
        this.ParentForm.controls.four_one_text.setValue(this.ParentForm.controls.OwnerNameLine1.value);

        // @ts-ignore
        const assessmentYear = document.getElementById('TwoCharAssessmentYear').value;

        // @ts-ignore
        this.ParentForm.controls.four_four_text.setValue(
            (assessmentYear.length > 0)
                ? '20' + assessmentYear : ''
        );
        // @ts-ignore
        this.ParentForm.controls.six_one_text.setValue(
            (assessmentYear.length > 0)
                ? '20' + assessmentYear : ''
        );

        this.ParentForm.controls.six_two_text.setValue(this.ParentForm.controls.LandVal.value);
        this.ParentForm.controls.six_three_text.setValue(this.ParentForm.controls.TotalVal.value);

        this.changeDetector.detectChanges();
    }
}
