import { TaskPriority } from './task-priority';
import { TaskStatus } from './task-status';

export interface TaskItem {
  taskId: number;
  title: string;
  description: string | null;
  priority: TaskPriority;
  createdDate: string;
  startDate: string | null;
  endDate: string | null;
  dueDate: string;
  status: TaskStatus;
  userId: number;
  userName: string;
}
