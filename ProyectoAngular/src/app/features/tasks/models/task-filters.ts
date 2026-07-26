import { TaskPriority } from './task-priority';
import { TaskStatus } from './task-status';

export interface TaskFilters {
  priority?: TaskPriority;
  status?: TaskStatus;
  userId?: number;
  page: number;
  pageSize: number;
}
