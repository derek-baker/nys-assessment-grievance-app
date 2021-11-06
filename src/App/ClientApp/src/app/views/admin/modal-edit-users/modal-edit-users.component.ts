import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { HttpAdminService } from 'src/app/services/http.service.admin';
import { User } from 'src/app/types/User';
import { SelectedGrievanceObservableService } from 'src/app/services/selected-application.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'app-modal-edit-users',
    templateUrl: './modal-edit-users.component.html',
    styleUrls: ['./modal-edit-users.component.css']
})
export class ModalEditUsersComponent implements OnInit, OnChanges {

    @Input()
    public IsOpen = false;
    public IsCreateUserWidgetOpen = false;
    public IsCreatingUserAtApi = false;

    public Users: Array<User> = [];

    public Username: string;
    public readonly Form: FormGroup;

    constructor(
        private readonly httpAdmin: HttpAdminService,
        private readonly selectedGrievanceService: SelectedGrievanceObservableService
    ) {
        // const validators = [Validators.required];
        // this.Form = new FormGroup({
        //     UserName: new FormControl('')
        // });
    }

    public ngOnInit(): void {

    }

    public ngOnChanges(changes: SimpleChanges & { IsOpen: {currentValue: boolean, previousValue?: boolean, firstChange: boolean}}){
        console.log('changes')
        console.log(changes)

        if (changes.IsOpen.currentValue === true) {
            this.onOpen();
        }
    }

    public onOpen() {
        this.httpAdmin.GetUsers().subscribe(
            (users) => {
                this.Users = users;

                console.log('onOpen')
                console.log(this.Users)
            });
    }

    public OpenCreateUserWidget() {
        this.IsCreateUserWidgetOpen = true;
    }

    public CreateUser() {
        this.IsCreatingUserAtApi = true;
        this.httpAdmin.CreateUser({userName: this.Username}).subscribe(
            () => {
                this.httpAdmin.GetUsers().subscribe(
                    (users) => { this.Users = users; }
                );
            },
            (err) => { console.error(err); },
            () => { this.IsCreatingUserAtApi = false; }
        );
    }

    public CancelCreate() {
        this.IsCreateUserWidgetOpen = false;
    }

    public DeleteUser(userId) {
        console.log('delete')
        // this.httpAdmin.TODO
    }

    public Clean() {
        this.IsOpen = false;
        this.IsCreateUserWidgetOpen = false;
    }
}
