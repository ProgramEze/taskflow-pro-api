import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Comment, CreateCommentRequest, UpdateCommentRequest } from '../models/comment.models';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly api = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getByTask(taskId: string) {
    return this.http.get<Comment[]>(`${this.api}/tasks/${taskId}/comments`);
  }

  create(taskId: string, data: CreateCommentRequest) {
    return this.http.post<Comment>(`${this.api}/tasks/${taskId}/comments`, data);
  }

  update(commentId: string, data: UpdateCommentRequest) {
    return this.http.put<Comment>(`${this.api}/comments/${commentId}`, data);
  }

  delete(commentId: string) {
    return this.http.delete<void>(`${this.api}/comments/${commentId}`);
  }
}
