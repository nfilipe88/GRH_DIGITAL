// src/app/pages/gestao-permissoes/gestao-permissoes.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionService } from '../../services/permission.service';
import { Permission } from '../../interfaces/permission';

@Component({
  selector: 'app-gestao-permissoes',
  imports: [CommonModule, FormsModule],
  templateUrl: './gestao-permissoes.html',
  styleUrls: ['./gestao-permissoes.css']
})
export class GestaoPermissoesComponent implements OnInit {
  permissions: Permission[] = [];
  filteredPermissions: Permission[] = [];
  selectedPermission: Permission | null = null;

  // Filtros
  searchTerm: string = '';
  selectedModule: string = '';
  selectedCategory: string = '';

  // Estatísticas
  modules: string[] = [];
  categories: string[] = [];
  totalPermissions: number = 0;
  activePermissions: number = 0;

  constructor(private permissionService: PermissionService) {}

  ngOnInit(): void {
    this.loadPermissions();
  }

  loadPermissions(): void {
    this.permissionService.getAllPermissions().subscribe({
      next: (permissions) => {
        this.permissions = permissions;
        this.filteredPermissions = permissions;
        this.calculateStatistics();
        this.extractFilters();
      },
      error: (err) => {
        console.error('Erro ao carregar permissões:', err);
      }
    });
  }

  calculateStatistics(): void {
    this.totalPermissions = this.permissions.length;
    this.activePermissions = this.permissions.filter(p => p.isActive).length;
  }

  extractFilters(): void {
    this.modules = [...new Set(this.permissions.map(p => p.module))];
    this.categories = [...new Set(this.permissions.map(p => p.category))];
  }

  applyFilters(): void {
    let filtered = this.permissions;

    // Filtrar por termo de busca
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.code.toLowerCase().includes(term) ||
        p.name.toLowerCase().includes(term) ||
        p.description?.toLowerCase().includes(term) ||
        false
      );
    }

    // Filtrar por módulo
    if (this.selectedModule) {
      filtered = filtered.filter(p => p.module === this.selectedModule);
    }

    // Filtrar por categoria
    if (this.selectedCategory) {
      filtered = filtered.filter(p => p.category === this.selectedCategory);
    }

    this.filteredPermissions = filtered;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedModule = '';
    this.selectedCategory = '';
    this.applyFilters();
  }

  togglePermissionStatus(permission: Permission): void {
    // Aqui você implementaria a lógica para ativar/desativar permissões
    console.log('Alternar status da permissão:', permission);
  }

  selectPermission(permission: Permission): void {
    this.selectedPermission = permission;
  }
}
