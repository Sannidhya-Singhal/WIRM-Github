import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CustomerOnboardingSubmitResponse } from '../models/dto/customer-onboarding-submit-response.dto';

@Injectable({
  providedIn: 'root',
})
export class CustomerOnboardingService {
  private readonly endpoint = `${environment.apiConfig.baseUrl}/api/CustomerOnboarding`;

  constructor(private readonly http: HttpClient) {}

  /**
   * multipart/form-data: `form` = JSON string of onboarding payload;
   * optional `otherUsersExcel` = single Excel file (Other Users).
   */
  submitCustomerOnboarding(payload: FormData): Observable<CustomerOnboardingSubmitResponse> {
    return this.http.post<CustomerOnboardingSubmitResponse>(this.endpoint, payload)
    .pipe(
      map(response => { 
        response.link = `${environment.baseAdoUri}${response.teamProject}/_workitems/edit/${response.id}`;
        return response;
      })
    );
  }
}
