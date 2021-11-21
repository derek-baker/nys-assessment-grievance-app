import { CanDeactivate } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface IComponentCanDeactivate {
    canDeactivate: () => boolean | Observable<boolean>;
}

@Injectable()
export class PendingChangesGuard implements CanDeactivate<IComponentCanDeactivate> {

    public canDeactivate(component: IComponentCanDeactivate): boolean | Observable<boolean> {
        return component.canDeactivate() ?
            true :
            // NOTE: this warning message will only be shown when navigating elsewhere within your angular app;
            // when navigating away from your angular app, the browser will show a generic warning message
            // see http://stackoverflow.com/a/42207299/7307355
            confirm(
                'WARNING: You have unsaved changes. Press Cancel to go back and save these changes, or OK to lose these changes.'
            );
    }
}
