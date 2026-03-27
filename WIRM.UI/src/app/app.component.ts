import { CommonModule } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MSAL_GUARD_CONFIG, MsalBroadcastService, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { AuthenticationResult, EventType, InteractionStatus, RedirectRequest } from '@azure/msal-browser';
import { filter, Subject, Subscription, takeUntil } from 'rxjs';
import { HeaderComponent } from './components/header/header.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  styleUrl: './app.component.css',
  imports: [CommonModule, RouterOutlet, HeaderComponent]
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'WIRM';
  accountName: any = null;
  msalSubscription: Subscription | undefined;
  private readonly _destroying$ = new Subject<void>();

  constructor(
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    private msalService: MsalService, 
    private msalBroadcastService: MsalBroadcastService
  ) { }
  
  async ngOnInit() {
    this.msalService.handleRedirectObservable().subscribe({
      next: (result) => {
        if (result) {
          this.setAccountName();
        }
      }, 
      error: (error) => {
        console.error('Redirect authentication failed.', error);
      }
    });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None), 
        takeUntil(this._destroying$)
      )
      .subscribe(() => {
        this.setAccountName();
      })
  }

  ngOnDestroy(): void {
    if (this.msalSubscription)
      this.msalSubscription.unsubscribe();
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  get isLoggedIn(): boolean {
    return this.msalService.instance.getAllAccounts().length > 0;
  }

  setAccountName() {
    const accounts = this.msalService.instance.getAllAccounts();
    if (accounts.length > 0) {
      this.accountName = accounts[0].name;
    }
  }

  async login() {
    if (this.msalGuardConfig.authRequest) {
      await this.msalService.loginRedirect({...this.msalGuardConfig.authRequest} as RedirectRequest);
    }
    else {
      await this.msalService.loginRedirect();
    }
  }

  logout() {
    this.msalService.logoutRedirect();
  }
}
