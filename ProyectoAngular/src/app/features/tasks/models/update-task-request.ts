import { TaskPriority } from './task-priority';
import { TaskStatus } from './task-status';

export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate: string;
  userId: number;
}
