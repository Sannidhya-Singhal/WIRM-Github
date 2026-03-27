import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { MsalModule, MsalService, MsalGuard, MsalBroadcastService, MsalInterceptor } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { environment } from './environments/environment';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    importProvidersFrom(
      MsalModule.forRoot(
        new PublicClientApplication({
          auth: {
            clientId: environment.msalConfig.clientId,
            authority: environment.msalConfig.authority,
            redirectUri: environment.msalConfig.redirectUri,
            postLogoutRedirectUri: environment.msalConfig.postLogoutRedirectUri
          },
          cache: {
            cacheLocation: BrowserCacheLocation.LocalStorage,
          },
          system: {
            loggerOptions: {
              loggerCallback: (level, message, containsPii) => {
                if (!containsPii) {
                  console.log(message);
                }
              },
              logLevel: environment.production ? 3 : 1,
              piiLoggingEnabled: !environment.production
            }
          }
        }),
        {
          interactionType: InteractionType.Redirect,
          authRequest: {
            scopes: environment.apiConfig.scopes
          }
        },
        {
          interactionType: InteractionType.Redirect,
          protectedResourceMap: new Map([
            [environment.apiConfig.baseUrl, environment.apiConfig.scopes]
          ])
        }
      )
    ),
    provideHttpClient(withInterceptorsFromDi()),
    {
        provide: HTTP_INTERCEPTORS,
        useClass: MsalInterceptor,
        multi: true
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ]
}).catch(err => console.error(err));