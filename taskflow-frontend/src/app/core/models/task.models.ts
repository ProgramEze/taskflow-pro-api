export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  createdByUserId: string;
  assignedUserId?: string;
  createdAt: string;
  dueDate?: string;
  isActive: boolean;
}

export enum TaskItemStatus {
  Pending   = 1,
  InProgress = 2,
  InReview  = 3,
  Completed = 4,
  Cancelled = 5
}

export const TaskStatusLabel: Record<TaskItemStatus, string> = {
  [TaskItemStatus.Pending]:    'Pendiente',
  [TaskItemStatus.InProgress]: 'En progreso',
  [TaskItemStatus.InReview]:   'En revisión',
  [TaskItemStatus.Completed]:  'Completada',
  [TaskItemStatus.Cancelled]:  'Cancelada'
};

export enum TaskPriority {
  Low    = 1,
  Medium = 2,
  High   = 3,
  Urgent = 4
}

export const TaskPriorityLabel: Record<TaskPriority, string> = {
  [TaskPriority.Low]:    'Baja',
  [TaskPriority.Medium]: 'Media',
  [TaskPriority.High]:   'Alta',
  [TaskPriority.Urgent]: 'Urgente'
};

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
}

export interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TaskQueryParams {
  status?: TaskItemStatus;
  priority?: TaskPriority;
  assignedUserId?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}
