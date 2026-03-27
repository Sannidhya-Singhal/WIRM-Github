import { LogLevel } from "@azure/msal-browser";

export const environment = {
    logLevel: LogLevel.Verbose,
    baseAdoUri: 'https://liox-teams.visualstudio.com/',
    apiConfig: {
        baseUrl: "https://opr-form-api-aecncneefgbceyga.eastus-01.azurewebsites.net",        
        scopes: [ "api://0ed17343-d316-4151-98a1-d8b0618a3525/User.Read" ],    
    },
     msalConfig: {
        clientId: "adcdab4d-0e62-4bb7-a102-0cd0c3e29bca",
        tenantId: "42dc8b0f-4759-4afe-9348-41952eeaf98b",
        authority: "https://login.microsoftonline.com/42dc8b0f-4759-4afe-9348-41952eeaf98b",
        redirectUri: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
    },
    production: true,
 }