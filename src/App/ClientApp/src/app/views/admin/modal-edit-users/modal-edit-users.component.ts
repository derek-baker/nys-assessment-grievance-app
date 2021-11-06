import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { HttpAdminService } from 'src/app/services/http.service.admin';
import { User } from 'src/app/types/User';
import { FormGroup } from '@angular/forms';

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
    public IsCreatingUserAtApi = false;

    public Users: Array<User> = [];

    public Username: string;
    public readonly Form: FormGroup;

    constructor(private readonly httpAdmin: HttpAdminService) {}

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
        this.IsCreatingUserAtApi = true;
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
                this.IsCreatingUserAtApi = false;
            },
            (err) => {
                console.error(err);
                this.IsCreatingUserAtApi = false;
                window.alert('An error occurred.');
            }
        );
    }

    public CancelCreate() {
        this.closeCreateUserDialog();
    }

    public DeleteUser(userId) {
        this.httpAdmin.DeleteUser(userId).subscribe(
            () => { this.getUsers(); },
            (err) => { console.error(err); window.alert('An error occurred.'); }
        );
    }

    public Clean() {
        this.IsOpen = false;
        this.closeCreateUserDialog();
    }

    private closeCreateUserDialog() {
        this.IsCreateUserWidgetOpen = false;
    }

    private getUsers() {
        this.httpAdmin.GetUsers().subscribe(
            (users) => { this.Users = users; },
            (err) => { console.error(err); window.alert('An error occurred.'); }
        );
    }
}
