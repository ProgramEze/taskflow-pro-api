import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CreateProjectRequest, Project } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly api = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getByWorkspace(workspaceId: string) {
    return this.http.get<Project[]>(`${this.api}/workspaces/${workspaceId}/projects`);
  }

  getById(id: string) {
    return this.http.get<Project>(`${this.api}/projects/${id}`);
  }

  create(workspaceId: string, data: CreateProjectRequest) {
    return this.http.post<Project>(`${this.api}/workspaces/${workspaceId}/projects`, data);
  }
}
