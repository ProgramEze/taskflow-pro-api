import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'workspaces',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'workspaces',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workspaces/workspace-list/workspace-list.component').then(m => m.WorkspaceListComponent)
  },
  {
    path: 'workspaces/:workspaceId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workspaces/workspace-detail/workspace-detail.component').then(m => m.WorkspaceDetailComponent)
  },
  {
    path: 'workspaces/:workspaceId/projects/:projectId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent)
  },
  {
    path: 'tasks/:taskId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/task-detail/task-detail.component').then(m => m.TaskDetailComponent)
  },
  {
    path: '**',
    redirectTo: 'workspaces'
  }
];
