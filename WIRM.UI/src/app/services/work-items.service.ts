import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, debounceTime, distinctUntilChanged, map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResponse, PaginationRequest } from '../models/pagination.model';
import { WorkitemPreview } from '../models/dto/workitem-preview.dto';
import { WorkItemCreateResponse } from '../models/dto/workitem-create-response.dto';
import { WorkitemSearch } from '../models/dto/workitem-search.dto';

@Injectable({
  providedIn: 'root'
})
export class WorkItemsService {
  asObservable() {
    throw new Error('Method not implemented.');
  }
  private searchSubject = new BehaviorSubject<string>('');

  constructor(private http: HttpClient) { }

  getWorkItems(request: PaginationRequest): Observable<PaginatedResponse<WorkitemPreview>> {
    let params = new HttpParams()
      .set('currentPage', request.currentPage.toString())
      .set('pageSize', request.pageSize.toString());
    
    if (request.search) {
      params = params.set('search', request.search);
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }
    return this.http.get<PaginatedResponse<WorkitemPreview>>(`${environment.apiConfig.baseUrl}/api/WorkItems`, { params })
      .pipe(
        map(items => ({ 
            ...items, 
            data: items.data.map(workItem => ({
              ...workItem,
              link: `${environment.baseAdoUri}/${workItem.teamProject}/_workitems/edit/${workItem.id}/`
          }))
        }))
      );
  }

  searchWorkItems(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  getSearchResults(): Observable<string> {
    return this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    );
  }

  getWorkItemDetails(id: string): Observable<WorkitemSearch> {
    return this.http.get<WorkitemSearch>(`${environment.apiConfig.baseUrl}/api/WorkItems/${id}`);
  }

  createTicket(payload: FormData): Observable<WorkItemCreateResponse> {    
    return this.http.post<WorkItemCreateResponse>(`${environment.apiConfig.baseUrl}/api/WorkItems`, payload)
      .pipe(
        map(workItem => {
          workItem.link = `${environment.baseAdoUri}/${workItem.teamProject}/_workitems/edit/${workItem.id}`;
          return workItem;
        })
      );
  }
}
