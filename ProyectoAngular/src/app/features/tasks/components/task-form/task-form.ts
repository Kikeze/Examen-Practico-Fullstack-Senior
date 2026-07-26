import { Component,EventEmitter,Input,OnChanges,OnInit,Output,SimpleChanges,inject,signal,effect } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder,ReactiveFormsModule,Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { ErrorResponse } from '../../../../shared/models/error-response';
import { CreateTaskRequest } from '../../models/create-task-request';
import { TaskItem } from '../../models/task-item';
import { TaskPriority } from '../../models/task-priority';
import { TaskStatus } from '../../models/task-status';
import { UpdateTaskRequest } from '../../models/update-task-request';
import { TaskService } from '../../services/task.service';
import { ActiveUser } from '../../../users/models/active-user';
import { UserService } from '../../../users/services/user.service';

@Component({
  selector: 'app-task-form',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './task-form.html',
  styleUrl: './task-form.css',
})
export class TaskForm implements OnInit, OnChanges {
  private readonly formBuilder = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly userService = inject(UserService);

  @Input()
  task: TaskItem | null = null;

  @Output()
  readonly saved = new EventEmitter<void>();

  @Output()
  readonly cancelled = new EventEmitter<void>();

  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly users = signal<ActiveUser[]>([]);
  readonly isLoadingUsers = signal(false);
  readonly usersErrorMessage = signal<string | null>(null);

  readonly priorities = [
    { value: TaskPriority.High, text: 'Alta' },
    { value: TaskPriority.Medium, text: 'Media' },
    { value: TaskPriority.Low, text: 'Baja' }
  ];

  readonly statuses = [
    { value: TaskStatus.Pending, text: 'Pendiente' },
    { value: TaskStatus.InProgress, text: 'En progreso' },
    { value: TaskStatus.Completed, text: 'Completada' }
  ];

  readonly form = this.formBuilder.nonNullable.group({
    title: [
      '',
      [
        Validators.required,
        Validators.maxLength(150)
      ]
    ],
    description: [
      '',
      [
        Validators.maxLength(500)
      ]
    ],
    priority: [
      TaskPriority.Medium,
      [
        Validators.required
      ]
    ],
    status: [
      TaskStatus.Pending,
      [
        Validators.required
      ]
    ],
    dueDate: [
      '',
      [
        Validators.required
      ]
    ],
    userId: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ]
  });

  constructor() {
    effect(() => { this.updateFormControlState(); });
  }

  get isEditMode(): boolean {
    return this.task !== null;
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task']) {
      this.loadTask();
    }
  }

  loadUsers(): void {
    this.isLoadingUsers.set(true);
    this.usersErrorMessage.set(null);

    this.userService
      .getActive()
      .pipe(finalize(() => this.isLoadingUsers.set(false)))
      .subscribe({
        next: users => {
          this.users.set(users);
          this.validateSelectedUser(users);
        },
        error: (error: HttpErrorResponse) => {
          this.usersErrorMessage.set(
            this.getUsersErrorMessage(error)
          );
        }
      });
  }

  private validateSelectedUser(users: ActiveUser[]): void {
    const selectedUserId = this.form.controls.userId.value;

    if (selectedUserId === 0) {
      return;
    }

    const userIsActive = users.some(user => user.userId === selectedUserId);

    if (!userIsActive) {
      this.form.controls.userId.setValue(0);
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

  submit(): void {
    this.errorMessage.set(null);

    if (this.isSaving() || this.isLoadingUsers() || this.usersErrorMessage() !== null || this.users().length === 0) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.getRawValue();

    if (this.isEditMode && this.task) {
      this.updateTask(this.task.taskId, formValue);
      return;
    }

    this.createTask(formValue);
  }

  cancel(): void {
    this.errorMessage.set(null);
    this.form.reset();

    this.cancelled.emit();
  }

  isInvalid(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];

    return control.invalid && (control.touched || control.dirty);
  }

  private loadTask(): void {
    this.errorMessage.set(null);

    if (!this.task) {
      this.form.reset({
        title: '',
        description: '',
        priority: TaskPriority.Medium,
        status: TaskStatus.Pending,
        dueDate: '',
        userId: 0
      });

      return;
    }

    this.form.reset({
      title: this.task.title,
      description: this.task.description ?? '',
      priority: this.task.priority,
      status: this.task.status,
      dueDate: this.toDateInputValue(this.task.dueDate),
      userId: this.task.userId
    });
  }

  private createTask(formValue: typeof this.form.value): void {
    const request: CreateTaskRequest = {
      title: formValue.title!.trim(),
      description: formValue.description?.trim() || null,
      priority: formValue.priority!,
      status: formValue.status!,
      dueDate: formValue.dueDate!,
      userId: formValue.userId!
    };

    this.isSaving.set(true);

    this.taskService
      .create(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.form.reset();
          this.saved.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(error)
          );
        }
      });
  }

  private updateTask(taskId: number, formValue: typeof this.form.value): void {
    const request: UpdateTaskRequest = {
      title: formValue.title!.trim(),
      description: formValue.description?.trim() || null,
      priority: formValue.priority!,
      status: formValue.status!,
      dueDate: formValue.dueDate!,
      userId: formValue.userId!
    };

    this.isSaving.set(true);

    this.taskService
      .update(taskId, request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.saved.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(
            this.getErrorMessage(error)
          );
        }
      });
  }

  private toDateInputValue(date: string): string {
    return date.substring(0, 10);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'No fue posible establecer comunicacion con el WebAPI.';
    }

    const response = error.error as Partial<ErrorResponse> | null;

    if (response && typeof response.message === 'string' && response.message.trim().length > 0) {
      return response.message;
    }

    return this.isEditMode
      ? 'Ocurrio un error al actualizar la tarea.'
      : 'Ocurrio un error al crear la tarea.';
  }

  private updateFormControlState(): void {
    const isSaving = this.isSaving();
    const isLoadingUsers = this.isLoadingUsers();
    const hasAvailableUsers = this.users().length > 0;
    const hasUsersError = this.usersErrorMessage() !== null;

    if (isSaving) {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.enable({ emitEvent: false });

    if (isLoadingUsers || !hasAvailableUsers || hasUsersError) {
      this.form.controls.userId.disable({ emitEvent: false });
    }
  }
}
