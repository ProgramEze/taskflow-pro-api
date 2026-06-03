import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { WorkspaceService } from '../../../core/services/workspace.service';
import { ProjectService } from '../../../core/services/project.service';
import { MemberService } from '../../../core/services/member.service';
import { AuthService } from '../../../core/services/auth.service';
import { Workspace } from '../../../core/models/workspace.models';
import { Project } from '../../../core/models/project.models';
import { WorkspaceMember, WorkspaceRole, WorkspaceRoleLabel } from '../../../core/models/member.models';

@Component({
  selector: 'app-workspace-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, NavbarComponent, SpinnerComponent, EmptyStateComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <app-navbar />

      <main class="max-w-6xl mx-auto px-4 sm:px-6 py-8">

        <div class="mb-6">
          <a routerLink="/workspaces" class="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1 mb-3">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            Workspaces
          </a>
          <h1 class="text-2xl font-bold text-gray-900">{{ workspace?.name }}</h1>
          <p *ngIf="workspace?.description" class="text-gray-500 text-sm mt-0.5">{{ workspace?.description }}</p>
        </div>

        <app-spinner *ngIf="loading" />

        <div *ngIf="!loading" class="grid grid-cols-1 lg:grid-cols-3 gap-6">

          <!-- Proyectos -->
          <div class="lg:col-span-2">
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-lg font-semibold text-gray-900">Proyectos</h2>
              <button (click)="showProjectForm = !showProjectForm" class="btn-primary text-sm">
                + Nuevo proyecto
              </button>
            </div>

            <div *ngIf="showProjectForm" class="card p-4 mb-4">
              <form [formGroup]="projectForm" (ngSubmit)="createProject()" class="space-y-3">
                <input type="text" formControlName="name" class="input-field" placeholder="Nombre del proyecto" />
                <textarea formControlName="description" class="input-field resize-none" rows="2" placeholder="Descripción"></textarea>
                <p *ngIf="projectError" class="text-sm text-red-600">{{ projectError }}</p>
                <div class="flex gap-2">
                  <button type="submit" class="btn-primary text-sm" [disabled]="creatingProject">{{ creatingProject ? 'Creando...' : 'Crear' }}</button>
                  <button type="button" class="btn-secondary text-sm" (click)="showProjectForm = false">Cancelar</button>
                </div>
              </form>
            </div>

            <app-empty-state *ngIf="projects.length === 0" title="Sin proyectos" description="Creá el primer proyecto del workspace." />

            <div class="space-y-3">
              <a
                *ngFor="let p of projects"
                [routerLink]="['/workspaces', workspaceId, 'projects', p.id]"
                class="card p-4 flex items-center justify-between hover:border-primary-300 hover:shadow-sm transition-all cursor-pointer block"
              >
                <div>
                  <h3 class="font-medium text-gray-900">{{ p.name }}</h3>
                  <p class="text-sm text-gray-500">{{ p.description || 'Sin descripción' }}</p>
                </div>
                <svg class="w-5 h-5 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </a>
            </div>
          </div>

          <!-- Miembros -->
          <div>
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-lg font-semibold text-gray-900">Miembros</h2>
              <button (click)="showMemberForm = !showMemberForm" class="btn-secondary text-sm">
                + Agregar
              </button>
            </div>

            <div *ngIf="showMemberForm" class="card p-4 mb-4">
              <form [formGroup]="memberForm" (ngSubmit)="addMember()" class="space-y-3">
                <input type="email" formControlName="email" class="input-field" placeholder="email@ejemplo.com" />
                <p *ngIf="memberError" class="text-sm text-red-600">{{ memberError }}</p>
                <div class="flex gap-2">
                  <button type="submit" class="btn-primary text-sm" [disabled]="addingMember">{{ addingMember ? 'Agregando...' : 'Agregar' }}</button>
                  <button type="button" class="btn-secondary text-sm" (click)="showMemberForm = false">Cancelar</button>
                </div>
              </form>
            </div>

            <div class="card divide-y divide-gray-100">
              <div *ngFor="let m of members" class="px-4 py-3 flex items-center justify-between">
                <div>
                  <p class="text-sm font-medium text-gray-900">{{ m.firstName }} {{ m.lastName }}</p>
                  <p class="text-xs text-gray-500">{{ m.email }}</p>
                </div>
                <span [class]="roleBadge(m.role)">{{ roleLabel(m.role) }}</span>
              </div>
              <div *ngIf="members.length === 0" class="px-4 py-8 text-center text-sm text-gray-400">Sin miembros</div>
            </div>
          </div>

        </div>
      </main>
    </div>
  `
})
export class WorkspaceDetailComponent implements OnInit {
  workspaceId = '';
  workspace?: Workspace;
  projects: Project[] = [];
  members: WorkspaceMember[] = [];
  loading = true;

  showProjectForm = false;
  creatingProject = false;
  projectError = '';
  projectForm: FormGroup;

  showMemberForm = false;
  addingMember = false;
  memberError = '';
  memberForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private workspaceService: WorkspaceService,
    private projectService: ProjectService,
    private memberService: MemberService,
    private auth: AuthService,
    private fb: FormBuilder
  ) {
    this.projectForm = this.fb.group({ name: ['', Validators.required], description: [''] });
    this.memberForm  = this.fb.group({ email: ['', [Validators.required, Validators.email]] });
  }

  ngOnInit() {
    this.workspaceId = this.route.snapshot.paramMap.get('workspaceId')!;
    this.load();
  }

  load() {
    this.loading = true;
    this.workspaceService.getById(this.workspaceId).subscribe(ws => { this.workspace = ws; });
    this.projectService.getByWorkspace(this.workspaceId).subscribe(p => { this.projects = p; });
    this.memberService.getByWorkspace(this.workspaceId).subscribe(m => { this.members = m; this.loading = false; });
  }

  createProject() {
    if (this.projectForm.invalid) return;
    this.creatingProject = true;
    this.projectError = '';
    this.projectService.create(this.workspaceId, this.projectForm.value).subscribe({
      next: (p) => { this.projects.unshift(p); this.projectForm.reset(); this.showProjectForm = false; this.creatingProject = false; },
      error: (err) => { this.projectError = err?.error?.message ?? 'Error al crear.'; this.creatingProject = false; }
    });
  }

  addMember() {
    if (this.memberForm.invalid) return;
    this.addingMember = true;
    this.memberError = '';
    this.memberService.add(this.workspaceId, this.memberForm.value).subscribe({
      next: (m) => { this.members.push(m); this.memberForm.reset(); this.showMemberForm = false; this.addingMember = false; },
      error: (err) => { this.memberError = err?.error?.message ?? 'Error al agregar.'; this.addingMember = false; }
    });
  }

  roleBadge(role: WorkspaceRole): string {
    const map: Record<WorkspaceRole, string> = {
      [WorkspaceRole.Owner]:  'badge badge-purple',
      [WorkspaceRole.Admin]:  'badge badge-blue',
      [WorkspaceRole.Member]: 'badge badge-gray'
    };
    return map[role] ?? 'badge badge-gray';
  }

  roleLabel(role: WorkspaceRole): string {
    return WorkspaceRoleLabel[role] ?? '';
  }
}
