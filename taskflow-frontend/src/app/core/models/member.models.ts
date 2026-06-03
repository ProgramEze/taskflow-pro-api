export interface WorkspaceMember {
  id: string;
  workspaceId: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: WorkspaceRole;
  joinedAt: string;
  isActive: boolean;
}

export enum WorkspaceRole {
  Owner = 1,
  Admin = 2,
  Member = 3
}

export const WorkspaceRoleLabel: Record<WorkspaceRole, string> = {
  [WorkspaceRole.Owner]: 'Owner',
  [WorkspaceRole.Admin]: 'Admin',
  [WorkspaceRole.Member]: 'Member'
};

export interface AddMemberRequest {
  email: string;
}

export interface UpdateMemberRoleRequest {
  role: WorkspaceRole;
}
