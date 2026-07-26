import { API_CONFIG } from './api.config';

export const API_ENDPOINTS = {
    tasks: `${API_CONFIG.baseUrl}/tasks`,
    reports: {
        pendingTasks: `${API_CONFIG.baseUrl}/reports/pending-tasks`
    },
    users: {
      active: `${API_CONFIG.baseUrl}/users/active`
    }
} as const;
