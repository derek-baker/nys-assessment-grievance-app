import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {

    public IsExpanded: boolean = false;

    constructor(private readonly router: Router) {
        //
    }

    public ngOnInit() {
        this.router.events
            .subscribe(
                (event) => {
                    if (event instanceof NavigationEnd) {
                        if (this.IsExpanded === true) {
                            // TODO: Refactor ID out.
                            // We want to dropdown to drop-UP after a navigation
                            document.getElementById('Nav_Hamburger').click();
                        }
                    }
                }
            );
    }

    public Toggle() {
        this.IsExpanded = !this.IsExpanded;
    }
}
