import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { WorkspaceService } from '../../../core/services/workspace.service';
import { Workspace } from '../../../core/models/workspace.models';

@Component({
  selector: 'app-workspace-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, NavbarComponent, SpinnerComponent, EmptyStateComponent],
  template: `
    <div class="min-h-screen bg-gray-50">
      <app-navbar />

      <main class="max-w-5xl mx-auto px-4 sm:px-6 py-8">

        <div class="flex items-center justify-between mb-6">
          <div>
            <h1 class="text-2xl font-bold text-gray-900">Mis Workspaces</h1>
            <p class="text-gray-500 text-sm mt-0.5">Gestioná tus espacios de trabajo</p>
          </div>
          <button (click)="showForm = !showForm" class="btn-primary">
            <svg class="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Nuevo workspace
          </button>
        </div>

        <!-- Formulario nuevo workspace -->
        <div *ngIf="showForm" class="card p-5 mb-6">
          <h2 class="text-base font-semibold text-gray-900 mb-4">Nuevo workspace</h2>
          <form [formGroup]="form" (ngSubmit)="create()" class="space-y-3">
            <input type="text" formControlName="name" class="input-field" placeholder="Nombre del workspace" />
            <textarea formControlName="description" class="input-field resize-none" rows="2" placeholder="Descripción (opcional)"></textarea>
            <p *ngIf="createError" class="text-sm text-red-600">{{ createError }}</p>
            <div class="flex gap-2">
              <button type="submit" class="btn-primary" [disabled]="creating">{{ creating ? 'Creando...' : 'Crear' }}</button>
              <button type="button" class="btn-secondary" (click)="showForm = false">Cancelar</button>
            </div>
          </form>
        </div>

        <app-spinner *ngIf="loading" />

        <app-empty-state
          *ngIf="!loading && workspaces.length === 0"
          title="Sin workspaces"
          description="Creá tu primer workspace para empezar."
        />

        <div *ngIf="!loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <a
            *ngFor="let ws of workspaces"
            [routerLink]="['/workspaces', ws.id]"
            class="card p-5 hover:border-primary-300 hover:shadow-md transition-all cursor-pointer block"
          >
            <div class="flex items-start justify-between mb-3">
              <div class="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center">
                <svg class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
            </div>
            <h3 class="font-semibold text-gray-900 mb-1">{{ ws.name }}</h3>
            <p class="text-sm text-gray-500 line-clamp-2">{{ ws.description || 'Sin descripción' }}</p>
          </a>
        </div>

      </main>
    </div>
  `
})
export class WorkspaceListComponent implements OnInit {
  workspaces: Workspace[] = [];
  loading = true;
  showForm = false;
  creating = false;
  createError = '';
  form: FormGroup;

  constructor(
    private workspaceService: WorkspaceService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name:        ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.workspaceService.getAll().subscribe({
      next: (ws) => { this.workspaces = ws; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  create() {
    if (this.form.invalid) return;
    this.creating = true;
    this.createError = '';
    this.workspaceService.create(this.form.value).subscribe({
      next: (ws) => {
        this.workspaces.unshift(ws);
        this.form.reset();
        this.showForm = false;
        this.creating = false;
      },
      error: (err) => {
        this.createError = err?.error?.message ?? 'Error al crear.';
        this.creating = false;
      }
    });
  }
}
