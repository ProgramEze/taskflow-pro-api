import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent, PriorityBadgeComponent } from '../../../shared/components/badges/badges.component';
import { ProjectService } from '../../../core/services/project.service';
import { TaskService } from '../../../core/services/task.service';
import { MemberService } from '../../../core/services/member.service';
import { AuthService } from '../../../core/services/auth.service';
import { Project } from '../../../core/models/project.models';
import { TaskItem, TaskItemStatus, TaskPriority, TaskStatusLabel, TaskPriorityLabel, PagedResponse } from '../../../core/models/task.models';
import { WorkspaceMember } from '../../../core/models/member.models';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule, NavbarComponent, SpinnerComponent, EmptyStateComponent, StatusBadgeComponent, PriorityBadgeComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <app-navbar />

      <main class="max-w-6xl mx-auto px-4 sm:px-6 py-8">

        <div class="mb-6">
          <a [routerLink]="['/workspaces', workspaceId]" class="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1 mb-3">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            Workspace
          </a>
          <h1 class="text-2xl font-bold text-gray-900">{{ project?.name }}</h1>
          <p *ngIf="project?.description" class="text-gray-500 text-sm mt-0.5">{{ project?.description }}</p>
        </div>

        <!-- Filtros -->
        <div class="card p-4 mb-5 flex flex-wrap gap-3">
          <select [(ngModel)]="filterStatus" (change)="loadTasks()" class="input-field w-auto text-sm">
            <option [ngValue]="undefined">Todos los estados</option>
            <option *ngFor="let s of statusOptions" [ngValue]="s.value">{{ s.label }}</option>
          </select>
          <select [(ngModel)]="filterPriority" (change)="loadTasks()" class="input-field w-auto text-sm">
            <option [ngValue]="undefined">Todas las prioridades</option>
            <option *ngFor="let p of priorityOptions" [ngValue]="p.value">{{ p.label }}</option>
          </select>
          <input [(ngModel)]="filterSearch" (input)="loadTasks()" type="text" class="input-field flex-1 min-w-[160px] text-sm" placeholder="Buscar tareas..." />
          <button (click)="showTaskForm = !showTaskForm" class="btn-primary text-sm ml-auto">
            + Nueva tarea
          </button>
        </div>

        <!-- Formulario nueva tarea -->
        <div *ngIf="showTaskForm" class="card p-4 mb-5">
          <form [formGroup]="taskForm" (ngSubmit)="createTask()" class="space-y-3">
            <input type="text" formControlName="title" class="input-field" placeholder="Título de la tarea" />
            <textarea formControlName="description" class="input-field resize-none" rows="2" placeholder="Descripción (opcional)"></textarea>
            <div class="flex gap-3">
              <select formControlName="priority" class="input-field w-auto">
                <option *ngFor="let p of priorityOptions" [ngValue]="p.value">{{ p.label }}</option>
              </select>
            </div>
            <p *ngIf="taskError" class="text-sm text-red-600">{{ taskError }}</p>
            <div class="flex gap-2">
              <button type="submit" class="btn-primary text-sm" [disabled]="creatingTask">{{ creatingTask ? 'Creando...' : 'Crear tarea' }}</button>
              <button type="button" class="btn-secondary text-sm" (click)="showTaskForm = false">Cancelar</button>
            </div>
          </form>
        </div>

        <app-spinner *ngIf="loading" />

        <app-empty-state *ngIf="!loading && tasks.length === 0" title="Sin tareas" description="Creá la primera tarea del proyecto." />

        <div *ngIf="!loading" class="space-y-3">
          <a
            *ngFor="let task of tasks"
            [routerLink]="['/tasks', task.id]"
            class="card p-4 flex items-start justify-between hover:border-primary-300 hover:shadow-sm transition-all cursor-pointer block"
          >
            <div class="flex-1 min-w-0">
              <h3 class="font-medium text-gray-900 truncate">{{ task.title }}</h3>
              <p *ngIf="task.description" class="text-sm text-gray-500 mt-0.5 truncate">{{ task.description }}</p>
              <div class="flex items-center gap-2 mt-2">
                <app-status-badge [status]="task.status" />
                <app-priority-badge [priority]="task.priority" />
              </div>
            </div>
            <svg class="w-5 h-5 text-gray-400 flex-shrink-0 ml-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </a>
        </div>

        <!-- Paginación -->
        <div *ngIf="paged && paged.totalPages > 1" class="flex items-center justify-center gap-3 mt-6">
          <button (click)="prevPage()" [disabled]="!paged.hasPreviousPage" class="btn-secondary text-sm">
            Anterior
          </button>
          <span class="text-sm text-gray-600">Página {{ paged.pageNumber }} de {{ paged.totalPages }}</span>
          <button (click)="nextPage()" [disabled]="!paged.hasNextPage" class="btn-secondary text-sm">
            Siguiente
          </button>
        </div>

      </main>
    </div>
  `
})
export class ProjectDetailComponent implements OnInit {
  workspaceId = '';
  projectId   = '';
  project?: Project;
  tasks: TaskItem[] = [];
  members: WorkspaceMember[] = [];
  paged?: PagedResponse<TaskItem>;
  loading = true;
  currentPage = 1;

  filterStatus?:   TaskItemStatus;
  filterPriority?: TaskPriority;
  filterSearch  = '';

  showTaskForm  = false;
  creatingTask  = false;
  taskError     = '';
  taskForm: FormGroup;

  statusOptions   = Object.entries(TaskStatusLabel).map(([v, l]) => ({ value: +v as TaskItemStatus, label: l }));
  priorityOptions = Object.entries(TaskPriorityLabel).map(([v, l]) => ({ value: +v as TaskPriority, label: l }));

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private taskService: TaskService,
    private memberService: MemberService,
    private auth: AuthService,
    private fb: FormBuilder
  ) {
    this.taskForm = this.fb.group({
      title:       ['', Validators.required],
      description: [''],
      priority:    [TaskPriority.Medium]
    });
  }

  ngOnInit() {
    this.workspaceId = this.route.snapshot.paramMap.get('workspaceId')!;
    this.projectId   = this.route.snapshot.paramMap.get('projectId')!;
    this.projectService.getById(this.projectId).subscribe(p => { this.project = p; });
    this.memberService.getByWorkspace(this.workspaceId).subscribe(m => { this.members = m; });
    this.loadTasks();
  }

  loadTasks() {
    this.loading = true;
    this.taskService.getByProject(this.projectId, {
      status:    this.filterStatus,
      priority:  this.filterPriority,
      search:    this.filterSearch || undefined,
      pageNumber: this.currentPage
    }).subscribe({
      next: (res) => { this.paged = res; this.tasks = res.items; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  createTask() {
    if (this.taskForm.invalid) return;
    this.creatingTask = true;
    this.taskError = '';
    this.taskService.create(this.projectId, this.taskForm.value).subscribe({
      next: (t) => { this.tasks.unshift(t); this.taskForm.reset({ priority: TaskPriority.Medium }); this.showTaskForm = false; this.creatingTask = false; },
      error: (err) => { this.taskError = err?.error?.message ?? 'Error al crear.'; this.creatingTask = false; }
    });
  }

  prevPage() { if (this.paged?.hasPreviousPage) { this.currentPage--; this.loadTasks(); } }
  nextPage() { if (this.paged?.hasNextPage)     { this.currentPage++; this.loadTasks(); } }
}
