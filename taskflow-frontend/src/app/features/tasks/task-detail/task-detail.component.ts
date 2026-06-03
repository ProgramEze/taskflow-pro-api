import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { StatusBadgeComponent, PriorityBadgeComponent } from '../../../shared/components/badges/badges.component';
import { TaskService } from '../../../core/services/task.service';
import { CommentService } from '../../../core/services/comment.service';
import { MemberService } from '../../../core/services/member.service';
import { AuthService } from '../../../core/services/auth.service';
import { TaskItem, TaskItemStatus, TaskPriority, TaskStatusLabel, TaskPriorityLabel } from '../../../core/models/task.models';
import { Comment } from '../../../core/models/comment.models';
import { WorkspaceMember } from '../../../core/models/member.models';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, DatePipe, NavbarComponent, SpinnerComponent, StatusBadgeComponent, PriorityBadgeComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <app-navbar />

      <main class="max-w-3xl mx-auto px-4 sm:px-6 py-8">

        <app-spinner *ngIf="loading" />

        <ng-container *ngIf="!loading && task">

          <!-- Cabecera -->
          <div class="mb-6">
            <button (click)="goBack()" class="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1 mb-3">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
              Volver
            </button>
            <div class="flex items-start justify-between gap-4">
              <h1 class="text-2xl font-bold text-gray-900">{{ task.title }}</h1>
              <div class="flex gap-2 flex-shrink-0">
                <app-status-badge [status]="task.status" />
                <app-priority-badge [priority]="task.priority" />
              </div>
            </div>
            <p *ngIf="task.description" class="text-gray-600 mt-2">{{ task.description }}</p>
          </div>

          <!-- Detalle -->
          <div class="card p-5 mb-6 grid grid-cols-2 gap-4 text-sm">
            <div>
              <span class="text-gray-500">Estado</span>
              <div class="mt-1">
                <select (change)="changeStatus($event)" class="input-field text-sm py-1">
                  <option *ngFor="let s of statusOptions" [value]="s.value" [selected]="s.value === task.status">{{ s.label }}</option>
                </select>
              </div>
            </div>
            <div>
              <span class="text-gray-500">Prioridad</span>
              <div class="mt-1">
                <select (change)="changePriority($event)" class="input-field text-sm py-1">
                  <option *ngFor="let p of priorityOptions" [value]="p.value" [selected]="p.value === task.priority">{{ p.label }}</option>
                </select>
              </div>
            </div>
            <div>
              <span class="text-gray-500">Asignado a</span>
              <p class="mt-1 font-medium text-gray-900">{{ assignedName || 'Sin asignar' }}</p>
            </div>
            <div>
              <span class="text-gray-500">Creado</span>
              <p class="mt-1 font-medium text-gray-900">{{ task.createdAt | date:'dd/MM/yyyy' }}</p>
            </div>
            <div *ngIf="task.dueDate">
              <span class="text-gray-500">Vencimiento</span>
              <p class="mt-1 font-medium text-gray-900">{{ task.dueDate | date:'dd/MM/yyyy' }}</p>
            </div>
          </div>

          <!-- Asignar -->
          <div *ngIf="!task.assignedUserId" class="card p-5 mb-6">
            <h2 class="text-base font-semibold text-gray-900 mb-3">Asignar tarea</h2>
            <div class="flex gap-2">
              <button (click)="selfAssign()" class="btn-secondary text-sm" [disabled]="assigning">
                {{ assigning ? 'Asignando...' : 'Asignarme esta tarea' }}
              </button>
            </div>
            <p *ngIf="assignError" class="mt-2 text-sm text-red-600">{{ assignError }}</p>
          </div>

          <!-- Comentarios -->
          <div class="card p-5">
            <h2 class="text-base font-semibold text-gray-900 mb-4">Comentarios</h2>

            <div class="space-y-4 mb-5">
              <div *ngFor="let c of comments" class="flex gap-3">
                <div class="w-8 h-8 bg-primary-100 rounded-full flex items-center justify-center flex-shrink-0">
                  <span class="text-xs font-semibold text-primary-700">{{ initials(c.authorName) }}</span>
                </div>
                <div class="flex-1">
                  <div class="flex items-baseline gap-2 mb-1">
                    <span class="text-sm font-medium text-gray-900">{{ c.authorName }}</span>
                    <span class="text-xs text-gray-400">{{ c.createdAt | date:'dd/MM HH:mm' }}</span>
                  </div>

                  <div *ngIf="editingCommentId !== c.id">
                    <p class="text-sm text-gray-700">{{ c.content }}</p>
                    <div *ngIf="c.userId === currentUserId" class="flex gap-3 mt-1">
                      <button (click)="startEdit(c)" class="text-xs text-gray-400 hover:text-primary-600">Editar</button>
                      <button (click)="deleteComment(c.id)" class="text-xs text-gray-400 hover:text-red-600">Eliminar</button>
                    </div>
                  </div>

                  <div *ngIf="editingCommentId === c.id">
                    <textarea [(ngModel)]="editContent" class="input-field text-sm resize-none" rows="2"></textarea>
                    <div class="flex gap-2 mt-2">
                      <button (click)="saveEdit(c.id)" class="btn-primary text-xs px-3 py-1">Guardar</button>
                      <button (click)="cancelEdit()" class="btn-secondary text-xs px-3 py-1">Cancelar</button>
                    </div>
                  </div>
                </div>
              </div>

              <p *ngIf="comments.length === 0" class="text-sm text-gray-400 text-center py-4">Sin comentarios aún.</p>
            </div>

            <!-- Nuevo comentario -->
            <form [formGroup]="commentForm" (ngSubmit)="addComment()" class="flex gap-2">
              <input
                type="text"
                formControlName="content"
                class="input-field flex-1"
                placeholder="Escribí un comentario..."
              />
              <button type="submit" class="btn-primary" [disabled]="addingComment">Enviar</button>
            </form>
          </div>

        </ng-container>

      </main>
    </div>
  `,
  // ngModel para el textarea de edición
  providers: [DatePipe]
})
export class TaskDetailComponent implements OnInit {
  task?: TaskItem;
  comments: Comment[] = [];
  members: WorkspaceMember[] = [];
  loading = true;
  assigning = false;
  assignError = '';
  addingComment = false;

  editingCommentId: string | null = null;
  editContent = '';

  commentForm: FormGroup;

  statusOptions   = Object.entries(TaskStatusLabel).map(([v, l]) => ({ value: +v as TaskItemStatus, label: l }));
  priorityOptions = Object.entries(TaskPriorityLabel).map(([v, l]) => ({ value: +v as TaskPriority, label: l }));

  get currentUserId(): string {
    return this.auth.currentUser()?.userId ?? '';
  }

  get assignedName(): string {
    if (!this.task?.assignedUserId) return '';
    const m = this.members.find(m => m.userId === this.task!.assignedUserId);
    return m ? `${m.firstName} ${m.lastName}` : 'Usuario';
  }

  constructor(
    private route: ActivatedRoute,
    private taskService: TaskService,
    private commentService: CommentService,
    private memberService: MemberService,
    private auth: AuthService,
    private fb: FormBuilder
  ) {
    this.commentForm = this.fb.group({ content: ['', Validators.required] });
  }

  ngOnInit() {
    const taskId = this.route.snapshot.paramMap.get('taskId')!;
    this.taskService.getById(taskId).subscribe({
      next: (t) => {
        this.task = t;
        this.loading = false;
        this.commentService.getByTask(taskId).subscribe(c => { this.comments = c; });
      },
      error: () => { this.loading = false; }
    });
  }

  goBack() { history.back(); }

  changeStatus(event: Event) {
    const status = +(event.target as HTMLSelectElement).value as TaskItemStatus;
    this.taskService.changeStatus(this.task!.id, status).subscribe(t => { this.task = t; });
  }

  changePriority(event: Event) {
    const priority = +(event.target as HTMLSelectElement).value as TaskPriority;
    this.taskService.changePriority(this.task!.id, priority).subscribe(t => { this.task = t; });
  }

  selfAssign() {
    this.assigning = true;
    this.assignError = '';
    this.taskService.assign(this.task!.id, this.currentUserId).subscribe({
      next: (t) => { this.task = t; this.assigning = false; },
      error: (err) => { this.assignError = err?.error?.message ?? 'Error al asignar.'; this.assigning = false; }
    });
  }

  addComment() {
    if (this.commentForm.invalid) return;
    this.addingComment = true;
    this.commentService.create(this.task!.id, this.commentForm.value).subscribe({
      next: (c) => { this.comments.push(c); this.commentForm.reset(); this.addingComment = false; },
      error: () => { this.addingComment = false; }
    });
  }

  startEdit(c: Comment) {
    this.editingCommentId = c.id;
    this.editContent = c.content;
  }

  saveEdit(id: string) {
    this.commentService.update(id, { content: this.editContent }).subscribe(updated => {
      const i = this.comments.findIndex(c => c.id === id);
      if (i >= 0) this.comments[i] = updated;
      this.cancelEdit();
    });
  }

  cancelEdit() {
    this.editingCommentId = null;
    this.editContent = '';
  }

  deleteComment(id: string) {
    this.commentService.delete(id).subscribe(() => {
      this.comments = this.comments.filter(c => c.id !== id);
    });
  }

  initials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}
