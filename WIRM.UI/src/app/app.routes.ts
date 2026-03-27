import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { CustomerOnboardingFormComponent } from './components/customer-onboarding-form/customer-onboarding-form/customer-onboarding-form.component';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/work-items-list/work-items-list.component').then(i => i.WorkItemsListComponent),
    canActivate: [MsalGuard],
  },
  {
    path: 'ticket-form',
    loadComponent: () => import('./components/ticket-form/ticket-form.component').then(i => i.TicketFormComponent),
    canActivate: [MsalGuard],
  },
  {
    path: 'customer-onboarding',
    loadComponent: () => import('./components/customer-onboarding-form/customer-onboarding-form/customer-onboarding-form.component').then(i => i.CustomerOnboardingFormComponent),
    canActivate: [MsalGuard],
  },
  {
      path: '**',
      redirectTo: ''
  }
];
