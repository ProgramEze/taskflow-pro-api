import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen bg-gradient-to-br from-primary-50 to-white flex items-center justify-center p-4">
      <div class="w-full max-w-md">

        <div class="text-center mb-8">
          <div class="inline-flex items-center justify-center w-12 h-12 bg-primary-600 rounded-xl mb-4">
            <svg class="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </div>
          <h1 class="text-2xl font-bold text-gray-900">TaskFlow Pro</h1>
          <p class="text-gray-500 mt-1">Ingresá a tu cuenta</p>
        </div>

        <div class="card p-6">
          <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-4">

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                type="email"
                formControlName="email"
                class="input-field"
                [class.input-error]="isInvalid('email')"
                placeholder="tu@email.com"
              />
              <p *ngIf="isInvalid('email')" class="mt-1 text-xs text-red-600">Email requerido</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
              <input
                type="password"
                formControlName="password"
                class="input-field"
                [class.input-error]="isInvalid('password')"
                placeholder="••••••••"
              />
              <p *ngIf="isInvalid('password')" class="mt-1 text-xs text-red-600">Contraseña requerida</p>
            </div>

            <p *ngIf="error" class="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {{ error }}
            </p>

            <button type="submit" class="btn-primary w-full" [disabled]="loading">
              {{ loading ? 'Ingresando...' : 'Ingresar' }}
            </button>
          </form>

          <p class="text-center text-sm text-gray-500 mt-4">
            ¿No tenés cuenta?
            <a routerLink="/register" class="text-primary-600 font-medium hover:underline ml-1">Registrate</a>
          </p>
        </div>

      </div>
    </div>
  `
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  isInvalid(field: string): boolean {
    const c = this.form.get(field);
    return !!(c?.invalid && c?.touched);
  }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';

    this.auth.login(this.form.value).subscribe({
      next: () => this.router.navigate(['/workspaces']),
      error: (err) => {
        this.error = err?.error?.message ?? 'Credenciales incorrectas.';
        this.loading = false;
      }
    });
  }
}
