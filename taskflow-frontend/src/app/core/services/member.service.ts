import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AddMemberRequest, UpdateMemberRoleRequest, WorkspaceMember } from '../models/member.models';

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly api = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getByWorkspace(workspaceId: string) {
    return this.http.get<WorkspaceMember[]>(`${this.api}/workspaces/${workspaceId}/members`);
  }

  add(workspaceId: string, data: AddMemberRequest) {
    return this.http.post<WorkspaceMember>(`${this.api}/workspaces/${workspaceId}/members`, data);
  }

  updateRole(workspaceId: string, memberId: string, data: UpdateMemberRoleRequest) {
    return this.http.put<WorkspaceMember>(`${this.api}/workspaces/${workspaceId}/members/${memberId}/role`, data);
  }

  remove(workspaceId: string, memberId: string) {
    return this.http.delete<void>(`${this.api}/workspaces/${workspaceId}/members/${memberId}`);
  }
}
