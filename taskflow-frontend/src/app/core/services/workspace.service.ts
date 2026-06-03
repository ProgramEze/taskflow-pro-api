import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CreateWorkspaceRequest, Workspace } from '../models/workspace.models';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly api = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Workspace[]>(`${this.api}/Workspaces`);
  }

  getById(id: string) {
    return this.http.get<Workspace>(`${this.api}/Workspaces/${id}`);
  }

  create(data: CreateWorkspaceRequest) {
    return this.http.post<Workspace>(`${this.api}/Workspaces`, data);
  }
}
