import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'tasks'
  },
  {
    path: 'tasks',
    loadComponent: () => import('./features/tasks/pages/task-list/task-list').then(module => module.TaskList)
  },
  {
    path: 'reports/pending-tasks',
    loadComponent: () => import('./features/reports/pages/pending-task-report/pending-task-report').then(module => module.PendingTaskReport)
  },
  {
    path: '**',
    redirectTo: 'tasks'
  }
];
