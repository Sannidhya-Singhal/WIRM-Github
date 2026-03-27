import { LogLevel } from "@azure/msal-browser";

export const environment = {
    logLevel: LogLevel.Verbose,
    baseAdoUri: 'https://liox-teams.visualstudio.com/',
    apiConfig: {
        baseUrl: "https://opr-form-api-staging.azurewebsites.net",
        scopes: [ "api://abbbd0f6-dad9-49ed-8bf1-338b9a870141/User.Read" ],    
    },
     msalConfig: {
        clientId: "48f48b4c-8609-4854-991e-528b61f7adb5",
        tenantId: "42dc8b0f-4759-4afe-9348-41952eeaf98b",
        authority: "https://login.microsoftonline.com/42dc8b0f-4759-4afe-9348-41952eeaf98b",
        redirectUri: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
    },
    production: false,
 }