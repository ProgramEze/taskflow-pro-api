export interface Comment {
  id: string;
  taskId: string;
  userId: string;
  authorName: string;
  content: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateCommentRequest {
  content: string;
}

export interface UpdateCommentRequest {
  content: string;
}
