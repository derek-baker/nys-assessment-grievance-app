import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { HttpAdminService } from 'src/app/services/http.service.admin';
import { User } from 'src/app/types/User';
import { FormGroup } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
    selector: 'app-modal-edit-users',
    templateUrl: './modal-edit-users.component.html',
    styleUrls: ['./modal-edit-users.component.css']
})
export class ModalEditUsersComponent implements OnInit, OnChanges {

    @Input()
    public IsOpen = false;
    public IsCreateUserWidgetOpen = false;
    public IsCreateUserSuccessMessageShown = false;

    public SelectedUserId: string;
    public IsOperatingOnUser = false;

    public Users: Array<User> = [];

    public Username: string;
    public readonly Form: FormGroup;

    constructor(
        private readonly httpAdmin: HttpAdminService,
        private readonly spinner: NgxSpinnerService
    ) {}

    public ngOnInit(): void {

    }

    public ngOnChanges(changes: SimpleChanges & { IsOpen: {currentValue: boolean, previousValue?: boolean, firstChange: boolean}}){
        if (changes.IsOpen.currentValue === true) {
            this.onOpen();
        }
    }

    public onOpen() {
        this.getUsers();
    }

    public OpenCreateUserWidget() {
        this.IsCreateUserWidgetOpen = true;
    }

    public CreateUser() {
        this.spinner.show();

        this.httpAdmin.CreateUser({userName: this.Username}).subscribe(
            () => {
                this.httpAdmin.GetUsers().subscribe(
                    (users) => {
                        this.Users = users;
                        this.closeCreateUserDialog();
                        this.Username = undefined;
                        this.IsCreateUserSuccessMessageShown = true;
                        setTimeout(
                            () => { this.IsCreateUserSuccessMessageShown = false; },
                            10000);
                    }
                );
                this.spinner.hide();
            },
            (err) => {
                this.spinner.hide();
                console.error(err);
                window.alert('An error occurred.');
            }
        );
    }

    public CancelCreate() {
        this.closeCreateUserDialog();
    }

    public DeleteUser(userId) {
        this.IsOperatingOnUser = true;
        this.setSelectedUserId(userId);

        // TO DO: Force user to confirm delete
        this.httpAdmin.DeleteUser(userId).subscribe(
            () => {
                this.getUsers();
                this.IsOperatingOnUser = false;
            },
            (err) => {
                console.error(err);
                window.alert('An error occurred.');
                this.IsOperatingOnUser = false;
            }
        );
    }

    public ResetUserPassword(userId) {
        this.IsOperatingOnUser = true;
        this.setSelectedUserId(userId);

        this.httpAdmin.ResetUserPassword(userId).subscribe(
            () => {
                window.alert('The user\'s password was reset. The user will receive an email with more information.');
                this.IsOperatingOnUser = false;
            },
            (err) => {
                console.error(err);
                window.alert('An error occurred.');
                this.IsOperatingOnUser = false;
            }
        );
    }

    public Clean() {
        this.IsOpen = false;
        this.closeCreateUserDialog();
    }

    private closeCreateUserDialog() {
        this.IsCreateUserWidgetOpen = false;
        this.setSelectedUserId(undefined);
    }

    private getUsers() {
        this.httpAdmin.GetUsers().subscribe(
            (users) => { this.Users = users; },
            (err) => { console.error(err); window.alert('An error occurred.'); }
        );
    }

    private setSelectedUserId(id) {
        this.SelectedUserId = id;
    }
}
