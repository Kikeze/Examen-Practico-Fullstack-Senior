import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { ErrorResponse } from '../../../../shared/models/error-response';
import { PagedResult } from '../../../../shared/models/paged-result';
import { PendingTaskReport as PendingTaskReportModel } from '../../models/pending-task-report';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-pending-task-report',
  imports: [],
  templateUrl: './pending-task-report.html',
  styleUrl: './pending-task-report.css',
})
export class PendingTaskReport implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly destroyRef = inject(DestroyRef);

  readonly reports = signal<PendingTaskReportModel[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly totalRecords = signal(0);
  readonly totalPages = signal(0);
  readonly hasPreviousPage = signal(false);
  readonly hasNextPage = signal(false);
  readonly pageSizeOptions = [5, 10, 20, 50] as const;

  readonly firstVisibleRecord = computed(() => {
    if (this.totalRecords() === 0) {
      return 0;
    }

    return ((this.currentPage() - 1) * this.pageSize()) + 1;
  });

  readonly lastVisibleRecord = computed(() => {
    return Math.min(this.currentPage() * this.pageSize(), this.totalRecords());
  });

  readonly visiblePages = computed(() => {
    const totalPages = this.totalPages();
    const currentPage = this.currentPage();

    if (totalPages <= 0) {
      return [];
    }

    const maximumVisiblePages = 5;
    const halfRange = Math.floor(maximumVisiblePages / 2);
    let startPage = Math.max(1, currentPage - halfRange);
    let endPage = Math.min(totalPages, startPage + maximumVisiblePages - 1);

    startPage = Math.max(1, endPage - maximumVisiblePages + 1);

    return Array.from({ length: endPage - startPage + 1 }, (_, index) => startPage + index);
  });

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    if (this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.reportService
      .getPendingTasks({ page: this.currentPage(), pageSize: this.pageSize() })
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => { this.isLoading.set(false); }))
      .subscribe({
        next: result => {
          this.applyPagedResult(result);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.getErrorMessage(error));
        }
      });
  }

  changePageSize(event: Event): void {
    if (this.isLoading()) {
      return;
    }

    const select = event.target as HTMLSelectElement;
    const newPageSize = Number(select.value);

    if (!Number.isInteger(newPageSize) || newPageSize < 1 || newPageSize > 100) {
      return;
    }

    if (newPageSize === this.pageSize()) {
      return;
    }

    this.pageSize.set(newPageSize);
    this.currentPage.set(1);

    this.loadReport();
  }

  goToPage(page: number): void {
    if (this.isLoading()) {
      return;
    }

    if (!Number.isInteger(page) || page < 1 || page > this.totalPages() || page === this.currentPage()) {
      return;
    }

    this.currentPage.set(page);
    this.loadReport();
  }

  goToPreviousPage(): void {
    if (!this.hasPreviousPage()) {
      return;
    }

    this.goToPage(this.currentPage() - 1);
  }

  goToNextPage(): void {
    if (!this.hasNextPage()) {
      return;
    }

    this.goToPage(this.currentPage() + 1);
  }

  private applyPagedResult(result: PagedResult<PendingTaskReportModel>): void {
    if (result.items.length === 0 && result.totalRecords > 0 && result.page > result.totalPages) {
      this.currentPage.set(result.totalPages);
      this.loadReport();

      return;
    }

    this.reports.set(result.items);
    this.currentPage.set(result.page);
    this.totalRecords.set(result.totalRecords);
    this.totalPages.set(result.totalPages);
    this.hasPreviousPage.set(result.hasPreviousPage);
    this.hasNextPage.set(result.hasNextPage);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'No fue posible establecer comunicacion con el WebAPI.';
    }

    const response = error.error as Partial<ErrorResponse> | null;

    if (response && typeof response.message === 'string' && response.message.trim().length > 0) {
      return response.message;
    }

    return 'Ocurrio un error al consultar el reporte de tareas pendientes.';
  }

}
