import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AgGridModule } from 'ag-grid-angular';
import { CurrencyPipe } from '@angular/common';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './views/nav-menu/nav-menu.component';
import { HomeComponent } from './views/home/home.component';
import { FormComponent } from './views/rp524/form/form.component';
import { HeaderComponent } from './views/rp524/form/page1/header/header.component';
import { Part1Component } from './views/rp524/form/page1/part1/part1.component';
import { Part2Component } from './views/rp524/form/page2/part2/part2.component';
import { Part3Component } from './views/rp524/form/page3/part3/part3.component';
import { Part4Component } from './views/rp524/form/page4/part4/part4.component';
import { Part6Component } from './views/rp524/form/page4/part6/part6.component';
import { Part5Component } from './views/rp524/form/page4/part5/part5.component';
import { TextInputComponent } from './views/rp524/shared/text-input/text-input.component';
import { CheckboxComponent } from './views/rp524/shared/checkbox/checkbox.component';
import { TextInputNonstyledComponent } from './views/rp524/shared/text-input-nonstyled/text-input-nonstyled.component';
import { BoardOnlyComponent } from './views/rp524/form/page4/board-only/board-only.component';
import { FormControlsComponent } from './views/rp524/form/form-controls/form-controls/form-controls.component';
import { SubmitComponent } from './views/submit/submit/submit.component';
import { FakePostComponent } from './views/fake-post/fake-post/fake-post.component';
import { AdminComponent } from './views/admin/admin.component';
import { SubmissionContinueComponent } from './views/submission-continue/submission-continue/submission-continue.component';
import { HelpComponent } from './views/help/help.component';
import { IeBlockerComponent } from './views/ie-blocker/ie-blocker.component';
import { ModalAddFilesComponent } from './views/admin/modal-add-files/modal-add-files.component';
import { ModalCreateSubmissionComponent } from './views/admin/modal-create-grievance/modal-create-submission.component';
import { ModalOnlineBarReviewComponent } from './views/admin/modal-online-bar-review/modal-online-bar-review.component';
import { ModalEmailDispositionsComponent } from './views/admin/modal-generate-dispositions/modal-generate-dispositions.component';
import { ModalSubmissionFilesComponent } from './views/admin/modal-submission-details/modal-submission-details.component';
import { ModalEditGrievanceComponent } from './views/admin/modal-edit-grievance/modal-edit-grievance.component';
import { ModalExportReviewComponent } from './views/admin/modal-export-review/modal-export-review.component';
import { ModalSetRepresentativesComponent } from './views/admin/modal-set-representatives/modal-set-representatives.component';
import { AdminInstructionsComponent } from './views/admin/instructions/instructions.component';
import { AdminSettingsComponent } from './views/admin/modal-settings/settings.component';

import { HAS_ACCEPTED_RP524_TERMS, SESSION_HASH_KEY } from './tokens/client.storage.prefix.token';
import { AutofocusDirective } from './directives/autofocus.directive';
import { PendingChangesGuard } from './services/router-guard.service';
import { StagedFilesComponent } from './components/FileInputStagedFilesDisplay/staged-files.component';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        FormComponent,
        HeaderComponent,
        Part1Component,
        Part2Component,
        Part3Component,
        Part4Component,
        Part5Component,
        Part6Component,
        TextInputComponent,
        CheckboxComponent,
        TextInputNonstyledComponent,
        BoardOnlyComponent,
        FormControlsComponent,
        SubmitComponent,
        FakePostComponent,
        AdminComponent,
        SubmissionContinueComponent,
        HelpComponent,
        IeBlockerComponent,
        AutofocusDirective,
        ModalAddFilesComponent,
        ModalCreateSubmissionComponent,
        ModalOnlineBarReviewComponent,
        ModalEmailDispositionsComponent,
        ModalSubmissionFilesComponent,
        ModalEditGrievanceComponent,
        ModalExportReviewComponent,
        ModalSetRepresentativesComponent,
        AdminInstructionsComponent,
        AdminSettingsComponent,
        StagedFilesComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        AgGridModule.withComponents([]),
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forRoot(
            [
                { path: '', component: HomeComponent, pathMatch: 'full' },
                { path: 'rp524', component: FormComponent, canDeactivate: [PendingChangesGuard] },
                { path: 'rp524/:key', component: FormComponent, canDeactivate: [PendingChangesGuard] },
                { path: 'submit', component: SubmitComponent },
                { path: 'admin', component: AdminComponent },
                { path: 'continue-submission', component: SubmissionContinueComponent },
                { path: 'help', component: HelpComponent },
                { path: 'fakePost', component: FakePostComponent },
                { path: 'warning', component: IeBlockerComponent }
            ]
        ),
        BrowserAnimationsModule
    ],
    providers: [
        CurrencyPipe,
        PendingChangesGuard,
        { provide: HAS_ACCEPTED_RP524_TERMS, useValue: 'HAS_ACCEPTED_TERMS' },
        { provide: SESSION_HASH_KEY, useValue: 'SESSION_HASH_KEY' }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
