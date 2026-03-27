import { Component, OnDestroy, OnInit } from '@angular/core';
import { WorkItemsService } from '../../services/work-items.service';
import { BehaviorSubject, firstValueFrom, map, Observable, Subject, takeUntil } from 'rxjs';
import { PaginatedResponse, PaginationRequest } from '../../models/pagination.model';
import { WorkItem } from '../../models/workitem.model';
import { MinPipe } from '../../shared/pipes/min.pipe';
import { AsyncPipe, DatePipe } from '@angular/common';
import { WorkitemPreview } from '../../models/dto/workitem-preview.dto';

@Component({
  selector: 'app-work-items-list',
  standalone: true,
  templateUrl: './work-items-list.component.html',
  styleUrl: './work-items-list.component.css',
  imports: [MinPipe, AsyncPipe, DatePipe]
})
export class WorkItemsListComponent implements OnInit, OnDestroy {
  private workItemsSubject = new BehaviorSubject<WorkitemPreview[]>([]);
  private paginationSubject = new BehaviorSubject<PaginatedResponse<WorkitemPreview> | null>(null);
  workItems$ = this.workItemsSubject.asObservable();
  sortedList$!: Observable<WorkitemPreview[]>;
  paginationInfo$ = this.paginationSubject.asObservable();
  pageNumbers$ = this.paginationInfo$.pipe(
    map(pagination => {
      if (!pagination) return [];
      const totalPages = pagination.pageCount;
      const currentPage = pagination.currentPage;
      const delta = 2;

      const range = [];
      const rangeWithDots = [];
      
      for (let i = Math.max(2, currentPage - delta); i <= Math.min(totalPages - 1, currentPage + delta); i++) {
        range.push(i);
      }

      if (currentPage - delta > 2) {
        rangeWithDots.push(1, -1); // -1 = ellipsis
      } else {
        rangeWithDots.push(1);
      }

      rangeWithDots.push(...range);

      if (currentPage + delta < totalPages - 1) {
        rangeWithDots.push(-1, totalPages);
      } else {
        rangeWithDots.push(totalPages);
      }

      return rangeWithDots;
    })
  );
  
  loading = false;
  error: string | null = null;

  sortColumn: keyof WorkitemPreview | '' = '';
  sortDirection: 'asc' | 'desc' | '' = '';

  currentRequest: PaginationRequest = {
    currentPage: 1,
    pageSize: 10,
    search: '',
    sortBy: 'System.Id',
    sortDirection: 'desc'
  };

  searchString = '';
  pageSizeOptions = [10, 20, 50, 100];
  private destroy$ = new Subject<void>(); 

  constructor(private workItemService: WorkItemsService) { }

  ngOnInit(): void {
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.workItemService.getSearchResults()
      .pipe(takeUntil(this.destroy$))
      .subscribe(searchTerm => {
        this.currentRequest.search = searchTerm;
        this.currentRequest.currentPage = 1; 
        this.loadWorkItems();
      });
  }

  async loadWorkItems(): Promise<void> {
    this.loading = true;
    this.error = null;

    try {
      const response = await firstValueFrom(this.workItemService.getWorkItems(this.currentRequest));
      this.workItemsSubject.next(response.data);
      this.paginationSubject.next(response);
      this.sortedList$ = this.workItems$;
    } catch (error) {
      this.error = 'Failed to load work items. Please try again.';
      console.error('Error loading work items:', error);
    } finally {
      this.loading = false;
    }
  }

  sortData(column: keyof WorkitemPreview): void {

    if (this.sortColumn === column) {
      // Toggle sort direction
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 
                            this.sortDirection === 'desc' ? '' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    if (!this.sortColumn || this.sortDirection === '') {
      this.sortedList$ = this.workItems$;
      return;
    }

    this.sortedList$ = this.workItems$.pipe(
      map(workItems => [...workItems].sort((a, b) => {
        let valA = a[column];
        let valB = b[column];

        if (column === 'favorite') {
          return this.sortDirection === 'asc' ? (valA === valB ? 0 : (valA ? -1 : 1)) : (valA === valB ? 0 : (valA ? 1 : -1));
        }

        // For string status and name, case-insensitive compare
        if (typeof valA === 'string' && typeof valB === 'string') {
          valA = valA.toLowerCase();
          valB = valB.toLowerCase();
        }

        if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
        if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
        return 0;
      })
    ));
  }

  openLink(url: string): void {
    window.open(url, '_blank');    
  }

  toggleFavorite(item: WorkitemPreview): void {
    item.favorite = !item.favorite;
  }

  onSearchChange(searchString: string): void {
    this.searchString = searchString;
    this.workItemService.searchWorkItems(searchString);
  }

  onPageChange(page: number): void {
    this.currentRequest.currentPage = page;
    this.loadWorkItems();
  }

  onPageSizeChange(pageSize: number): void {
    this.currentRequest.pageSize = pageSize;
    this.currentRequest.currentPage = 1;
    this.loadWorkItems();
  }

}
