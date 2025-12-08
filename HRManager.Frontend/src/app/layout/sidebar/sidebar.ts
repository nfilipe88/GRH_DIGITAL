import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterLink, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  // 2. Receber a propriedade.
  // Por defeito, começa como 'true'
  @Input() isSidebarOpen: boolean = true;
  private authService = inject(AuthService);
  private router = inject(Router);

  isCollapsed = false;
  currentRoute = '';
  userRole: string | null = null;

  // Lista completa de menus (A "Matriz de Permissões")
  allMenuItems: MenuItem[] = [
    // --- Geral ---
    {
      label: 'Dashboard',
      icon: 'fas fa-home',
      route: '/dashboard',
      roles: ['*']
    },

    // --- Módulo Administrativo (SaaS) ---
    {
      label: 'Instituições',
      icon: 'fas fa-building',
      route: '/gestao-instituicoes',
      roles: ['GestorMaster']
    },
    {
      label: 'Utilizadores',
      icon: 'fas fa-users-cog',
      route: '/gestao-utilizadores',
      roles: ['GestorMaster']
    },

    // --- Módulo Gestão de RH (Operacional) ---
    {
      label: 'Colaboradores',
      icon: 'fas fa-users',
      route: '/gestao-colaboradores',
      roles: ['GestorMaster', 'GestorRH']
    },
    {
      label: 'Gestão Ausências',
      icon: 'fas fa-calendar-check',
      route: '/gestao-ausencias',
      roles: ['GestorMaster', 'GestorRH']
    },
    {
      label: 'Avaliações Equipa',
      icon: 'fas fa-chart-line',
      route: '/avaliacoes/minha-equipa',
      roles: ['GestorMaster', 'GestorRH']
    },
    {
      label: 'Calendário',
      icon: 'fas fa-calendar-alt',
      route: '/gestao-calendario',
      roles: ['GestorMaster', 'GestorRH']
    },
    {
      label: 'Relatórios',
      icon: 'fas fa-file-contract',
      route: '/emissao-declaracoes', // Ou rota de relatórios
      roles: ['GestorMaster', 'GestorRH']
    },

    // --- Área Pessoal (Self-Service - Todos têm) ---
    {
      label: 'Meu Perfil',
      icon: 'fas fa-id-card',
      route: '/perfil',
      roles: ['*']
    },
    {
      label: 'Minhas Ausências',
      icon: 'fas fa-umbrella-beach',
      route: '/minhas-ausencias',
      roles: ['*']
    },
    {
      label: 'Minhas Avaliações',
      icon: 'fas fa-star',
      route: '/minhas-avaliacoes', // Rota futura
      roles: ['*'] // Colaborador vê as suas, Gestor também é avaliado pelo seu superior
    },
    {
      label: 'Meus Documentos',
      icon: 'fas fa-folder-open',
      route: '/minhas-declaracoes',
      roles: ['*']
    }
  ];

  // Esta é a lista que será exibida no HTML
  visibleMenuItems: MenuItem[] = [];

  ngOnInit() {
    // 1. Obter a role do utilizador logado
    this.userRole = this.authService.getUserRole();

    // 2. Filtrar os menus
    this.filtrarMenus();

    // 3. Detetar rota ativa para destacar no menu
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentRoute = event.urlAfterRedirects;
    });
  }

  filtrarMenus() {
    if (!this.userRole) {
      this.visibleMenuItems = [];
      return;
    }

    this.visibleMenuItems = this.allMenuItems.filter(item => {
      // Se a role for '*', todos veem
      if (item.roles.includes('*')) return true;
      // Senão, verifica se a role do user está na lista permitida
      return item.roles.includes(this.userRole!);
    });
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }
}
