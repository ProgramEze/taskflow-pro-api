import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CreateTaskRequest, PagedResponse, TaskItem, TaskQueryParams, UpdateTaskRequest } from '../models/task.models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly api = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getByProject(projectId: string, query: TaskQueryParams = {}) {
    let params = new HttpParams();
    if (query.status    !== undefined) params = params.set('status',         query.status);
    if (query.priority  !== undefined) params = params.set('priority',       query.priority);
    if (query.assignedUserId)          params = params.set('assignedUserId', query.assignedUserId);
    if (query.search)                  params = params.set('search',         query.search);
    params = params.set('pageNumber', query.pageNumber ?? 1);
    params = params.set('pageSize',   query.pageSize   ?? 20);

    return this.http.get<PagedResponse<TaskItem>>(`${this.api}/projects/${projectId}/tasks`, { params });
  }

  getById(id: string) {
    return this.http.get<TaskItem>(`${this.api}/tasks/${id}`);
  }

  create(projectId: string, data: CreateTaskRequest) {
    return this.http.post<TaskItem>(`${this.api}/projects/${projectId}/tasks`, data);
  }

  update(id: string, data: UpdateTaskRequest) {
    return this.http.put<TaskItem>(`${this.api}/tasks/${id}`, data);
  }

  changeStatus(id: string, status: number) {
    return this.http.patch<TaskItem>(`${this.api}/tasks/${id}/status`, { status });
  }

  changePriority(id: string, priority: number) {
    return this.http.patch<TaskItem>(`${this.api}/tasks/${id}/priority`, { priority });
  }

  assign(id: string, assignedUserId: string) {
    return this.http.patch<TaskItem>(`${this.api}/tasks/${id}/assign`, { assignedUserId });
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.api}/tasks/${id}`);
  }
}
