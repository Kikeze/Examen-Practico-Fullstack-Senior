import { Component, DestroyRef, OnInit, inject, signal, effect, computed } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule} from '@angular/forms';

import { ErrorResponse } from '../../../../shared/models/error-response';
import { PagedResult } from '../../../../shared/models/paged-result';
import { TaskItem } from '../../models/task-item';
import { TaskPriority } from '../../models/task-priority';
import { TaskStatus } from '../../models/task-status';
import { TaskService } from '../../services/task.service';
import { TaskForm } from '../../components/task-form/task-form';
import { ActiveUser } from '../../../users/models/active-user';
import { UserService } from '../../../users/services/user.service';

@Component({
  selector: 'app-task-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    TaskForm
  ],
  templateUrl: './task-list.html',
  styleUrl: './task-list.css',
})
export class TaskList implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly userService = inject(UserService);

  readonly tasks = signal<TaskItem[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isFormVisible = signal(false);
  readonly selectedTask = signal<TaskItem | null>(null);
  readonly taskToDelete = signal<TaskItem | null>(null);
  readonly isDeleting = signal(false);
  readonly deleteErrorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly currentPage = signal(1);
  readonly pageSize = signal(10);
  readonly totalRecords = signal(0);
  readonly totalPages = signal(0);
  readonly hasPreviousPage = signal(false);
  readonly hasNextPage = signal(false);

  readonly filterForm = this.formBuilder.group({
    priority: this.formBuilder.control<TaskPriority | null>(null),
    status: this.formBuilder.control<TaskStatus | null>(null),
    userId: this.formBuilder.control<number | null>(null)
  });

  readonly appliedPriority = signal<TaskPriority | undefined>(undefined);
  readonly appliedStatus = signal<TaskStatus | undefined>(undefined);
  readonly appliedUserId = signal<number | undefined>(undefined);

  readonly users = signal<ActiveUser[]>([]);
  readonly isLoadingUsers = signal(false);
  readonly usersErrorMessage = signal<string | null>(null);

  readonly priorityOptions = [
    {value: TaskPriority.High, label: 'Alta'},
    {value: TaskPriority.Medium, label: 'Media'},
    {value: TaskPriority.Low, label: 'Baja'}
  ] as const;

  readonly statusOptions = [
    {value: TaskStatus.Pending, label: 'Pendiente'},
    {value: TaskStatus.InProgress, label: 'En progreso'},
    {value: TaskStatus.Completed, label: 'Completada'}
  ] as const;

  readonly pageSizeOptions = [5, 10, 20, 50] as const;

  readonly visiblePages = computed(() => {
    const totalPages = this.totalPages();
    const currentPage = this.currentPage();
    const maximumVisiblePages = 5;

    if (totalPages <= 0) {
      return [];
    }

    if (totalPages <= maximumVisiblePages) {
      return Array.from({ length: totalPages }, (_, index) => index + 1);
    }

    const half = Math.floor(maximumVisiblePages / 2);
    let startPage = currentPage - half;
    let endPage = currentPage + half;

    if (startPage < 1) {
      startPage = 1;
      endPage = maximumVisiblePages;
    }

    if (endPage > totalPages) {
      endPage = totalPages;
      startPage = totalPages - maximumVisiblePages + 1;
    }

    return Array.from({ length: endPage - startPage + 1 }, (_, index) => startPage + index);
  });

  readonly firstVisibleRecord = computed(() => {
    if (this.totalRecords() === 0 || this.tasks().length === 0) {
      return 0;
    }

    return ((this.currentPage() - 1) * this.pageSize()) + 1;
  });

  readonly lastVisibleRecord = computed(() => {
    if (this.totalRecords() === 0 || this.tasks().length === 0) {
      return 0;
    }

    return Math.min(this.currentPage() * this.pageSize(), this.totalRecords());
  });

  constructor() {
    effect(() => { this.updateFilterControlState(); });
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadTasks();
  }

  loadUsers(): void {
    this.isLoadingUsers.set(true);
    this.usersErrorMessage.set(null);

    this.userService
      .getActive()
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.isLoadingUsers.set(false)))
      .subscribe({
        next: users => {
          this.users.set(users);
        },
        error: (error: HttpErrorResponse) => {
          this.usersErrorMessage.set(
            this.getUsersErrorMessage(error)
          );
        }
      });
  }

  loadTasks(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.taskService
      .getAll({
        priority: this.appliedPriority(),
        status: this.appliedStatus(),
        userId: this.appliedUserId(),
        page: this.currentPage(),
        pageSize: this.pageSize() })
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: result => {
          this.applyPagedResult(result);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.getErrorMessage(error));
        }
      });
  }

  applyFilters(): void {
    if (this.isLoading() || this.isDeleting()) {
      return;
    }

    const filters = this.filterForm.getRawValue();

    this.appliedPriority.set(filters.priority ?? undefined);
    this.appliedStatus.set(filters.status ?? undefined);
    this.appliedUserId.set(filters.userId ?? undefined);
    this.currentPage.set(1);
    this.successMessage.set(null);

    this.loadTasks();
  }

  clearFilters(): void {
    if (this.isLoading() || this.isDeleting()) {
      return;
    }

    this.filterForm.reset({
      priority: null,
      status: null,
      userId: null
    });

    this.appliedPriority.set(undefined);
    this.appliedStatus.set(undefined);
    this.appliedUserId.set(undefined);

    this.currentPage.set(1);
    this.successMessage.set(null);

    this.loadTasks();
  }

  private updateFilterControlState(): void {
    const shouldDisable = this.isLoading() || this.isDeleting();

    if (shouldDisable) {
      this.filterForm.disable({ emitEvent: false });
      return;
    }

    this.filterForm.enable({ emitEvent: false });

    if (this.isLoadingUsers() || this.usersErrorMessage() !== null) {
      this.filterForm.controls.userId.disable({ emitEvent: false });
    }
  }

  changePageSize(event: Event): void {
    if (this.isLoading() || this.isDeleting()) {
      return;
    }

    const select = event.currentTarget as HTMLSelectElement;
    const newPageSize = Number.parseInt(select.value, 10);
    const isAllowedPageSize = this.pageSizeOptions.some(option => option === newPageSize);

    if (!isAllowedPageSize) {
      select.value = String(this.pageSize());
      return;
    }

    if (newPageSize === this.pageSize()) {
      return;
    }

    this.pageSize.set(newPageSize);
    this.currentPage.set(1);
    this.successMessage.set(null);

    this.loadTasks();
  }

  goToPage(page: number): void {
    if (this.isLoading() || this.isDeleting()) {
      return;
    }

    if (!Number.isInteger(page) || page < 1 || page > this.totalPages() || page === this.currentPage()) {
      return;
    }

    this.currentPage.set(page);
    this.successMessage.set(null);

    this.loadTasks();
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

  private applyPagedResult(result: PagedResult<TaskItem>): void {
    if (result.items.length === 0 && result.totalRecords > 0 && result.page > result.totalPages) {
      this.currentPage.set(result.totalPages);
      this.loadTasks();
      return;
    }

    this.tasks.set(result.items);
    this.currentPage.set(result.page);
    this.pageSize.set(result.pageSize);
    this.totalRecords.set(result.totalRecords);
    this.totalPages.set(result.totalPages);
    this.hasPreviousPage.set(result.hasPreviousPage);
    this.hasNextPage.set(result.hasNextPage);
  }

  getPriorityText(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.High:
        return 'Alta';
      case TaskPriority.Medium:
        return 'Media';
      case TaskPriority.Low:
        return 'Baja';
      default:
        return 'Desconocida';
    }
  }

  getPriorityBadgeClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.High:
        return 'text-bg-danger';
      case TaskPriority.Medium:
        return 'text-bg-warning';
      case TaskPriority.Low:
        return 'text-bg-success';
      default:
        return 'text-bg-secondary';
    }
  }

  getStatusText(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return 'Pendiente';
      case TaskStatus.InProgress:
        return 'En progreso';
      case TaskStatus.Completed:
        return 'Completada';
      default:
        return 'Desconocido';
    }
  }

  getStatusBadgeClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return 'text-bg-secondary';
      case TaskStatus.InProgress:
        return 'text-bg-primary';
      case TaskStatus.Completed:
        return 'text-bg-success';
      default:
        return 'text-bg-dark';
    }
  }

  private getUsersErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'No fue posible consultar la lista de usuarios.';
    }

    const response = error.error as Partial<ErrorResponse> | null;

    if (response && typeof response.message === 'string' && response.message.trim().length > 0) {
      return response.message;
    }

    return 'Ocurrio un error al consultar los usuarios activos.';
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'No fue posible establecer comunicacion con el WebAPI.';
    }

    const response = error.error as Partial<ErrorResponse> | null;

    if (response && typeof response.message === 'string' && response.message.trim().length > 0) {
      return response.message;
    }

    return 'Ocurrio un error al consultar las tareas.';
  }

  openCreateForm(): void {
    this.successMessage.set(null);
    this.selectedTask.set(null);
    this.isFormVisible.set(true);
  }

  openEditForm(task: TaskItem): void {
    this.successMessage.set(null);
    this.selectedTask.set(task);
    this.isFormVisible.set(true);
  }

  closeForm(): void {
    this.selectedTask.set(null);
    this.isFormVisible.set(false);
  }

  handleTaskSaved(): void {
    const wasEditing = this.selectedTask() !== null;

    const message = wasEditing
      ? 'La tarea fue actualizada correctamente.'
      : 'La tarea fue creada correctamente.';

    this.closeForm();
    this.successMessage.set(message);

    if (!wasEditing) {
      this.currentPage.set(1);
    }

    this.loadTasks();
  }

  deleteTask(): void {
    const task = this.taskToDelete();

    if (!task || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteErrorMessage.set(null);
    this.successMessage.set(null);

    this.taskService
      .delete(task.taskId)
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.taskToDelete.set(null);
          this.successMessage.set(`La tarea "${task.title}" fue eliminada correctamente.`);
          this.loadTasks();
        },
        error: (error: HttpErrorResponse) => {
          this.deleteErrorMessage.set(
            this.getDeleteErrorMessage(error)
          );
        }
      });
  }

  private getDeleteErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'No fue posible establecer comunicacion con el WebAPI.';
    }

    const response = error.error as Partial<ErrorResponse> | null;

    if (response && typeof response.message === 'string' && response.message.trim().length > 0) {
      return response.message;
    }

    if (error.status === 404) {
      return 'La tarea ya no existe o fue eliminada previamente.';
    }

    return 'Ocurrio un error al eliminar la tarea.';
  }

  openDeleteConfirmation(task: TaskItem): void {
    if (this.isDeleting()) {
      return;
    }

    this.successMessage.set(null);
    this.deleteErrorMessage.set(null);
    this.taskToDelete.set(task);
  }

  closeDeleteConfirmation(): void {
    if (this.isDeleting()) {
      return;
    }

    this.taskToDelete.set(null);
    this.deleteErrorMessage.set(null);
  }

}
