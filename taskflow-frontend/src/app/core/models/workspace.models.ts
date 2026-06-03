export interface Workspace {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateWorkspaceRequest {
  name: string;
  description?: string;
}
