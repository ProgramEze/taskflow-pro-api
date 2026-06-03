export interface Project {
  id: string;
  workspaceId: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  createdAt: string;
  dueDate?: string;
}

export enum ProjectStatus {
  Active = 1,
  Paused = 2,
  Completed = 3,
  Archived = 4
}

export const ProjectStatusLabel: Record<ProjectStatus, string> = {
  [ProjectStatus.Active]: 'Activo',
  [ProjectStatus.Paused]: 'Pausado',
  [ProjectStatus.Completed]: 'Completado',
  [ProjectStatus.Archived]: 'Archivado'
};

export interface CreateProjectRequest {
  name: string;
  description?: string;
  dueDate?: string;
}
