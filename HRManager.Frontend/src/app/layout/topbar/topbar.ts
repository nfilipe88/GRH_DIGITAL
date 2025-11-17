import { Component, EventEmitter, inject, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-topbar',
  imports: [CommonModule],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css',
})
export class Topbar {
  // 1. Criar o emissor de evento
  @Output() toggleSidebarEvent = new EventEmitter<void>();

  private authService = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // 2. Criar a função que emite o evento
  onToggleSidebar(): void {
    this.toggleSidebarEvent.emit();
  }
}
