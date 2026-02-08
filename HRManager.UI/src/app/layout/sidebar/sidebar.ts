import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { MenuPermissionService } from '../../services/menu-permission.service';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterLink, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input() isSidebarOpen: boolean = true;
  private authService = inject(AuthService);
  private router = inject(Router);
  private menuPermissionService = inject(MenuPermissionService);

  userRole: string[] = [];
  menuItems: MenuItem[] = [];

  ngOnInit(): void {
    this.menuItems = this.menuPermissionService.getMainMenu();
  }

  // --- Helpers de Permissão ---
  // Apenas para o Dono do Sistema (Vê Instituições e Utilizadores de Sistema)
  get isAdmin(): boolean {
    return this.userRole.includes('GestorMaster') || this.userRole.includes('GestorRH') || this.userRole.includes('Admin');
  }

  get isMaster(): boolean {
    return this.userRole.includes('GestorMaster') || this.userRole.includes('GestorRH') || this.userRole.includes('Colaborador');
  }

  // Para quem gere RH (Master ou Gestor de RH da empresa)
  // Vê Colaboradores, Configurações de Avaliação, Aprovações
  get isGestor(): boolean {
    return this.userRole.includes('GestorMaster') || this.userRole.includes('GestorRH');
  }

  // Todos veem (não precisa de getter, é o padrão), mas se quiseres isolar Colaboradores:
  get isColaborador(): boolean {
    const userRoles = this.authService.getUserRoles();
    return userRoles.includes('Colaborador') || this.isGestor;
  }

  hasChildren(item: MenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
