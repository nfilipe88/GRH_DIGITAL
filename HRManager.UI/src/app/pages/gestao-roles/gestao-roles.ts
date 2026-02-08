// src/app/pages/gestao-roles/gestao-roles.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-gestao-roles',
  imports: [CommonModule, FormsModule],
  templateUrl: './gestao-roles.html',
  styleUrls: ['./gestao-roles.css']
})
export class GestaoRoles implements OnInit {
  roles: any[] = [];
  roleSelecionada: any = null;
  permissoesAgrupadas: any[] = [];

  // Mock data para exemplo
  private mockRoles = [
    { id: 1, name: 'Administrador', description: 'Acesso total ao sistema' },
    { id: 2, name: 'Gestor RH', description: 'Gestão de recursos humanos' },
    { id: 3, name: 'Colaborador', description: 'Acesso básico' }
  ];

  private mockPermissoes = [
    { id: 1, name: 'USERS_VIEW', module: 'Usuários', category: 'Visualização' },
    { id: 2, name: 'USERS_EDIT', module: 'Usuários', category: 'Edição' },
    { id: 3, name: 'ROLES_VIEW', module: 'Roles', category: 'Visualização' }
  ];

  constructor() {}

  ngOnInit(): void {
    this.carregarRoles();
  }

  carregarRoles(): void {
    this.roles = this.mockRoles;
  }

  novaRole(): void {
    this.roleSelecionada = {
      id: null,
      name: '',
      description: '',
      permissoes: []
    };
  }

  selecionarRole(role: any): void {
    this.roleSelecionada = { ...role };
    this.agruparPermissoes();
  }

  agruparPermissoes(): void {
    // Agrupar por módulo
    const grupos: any = {};

    this.mockPermissoes.forEach(permissao => {
      if (!grupos[permissao.module]) {
        grupos[permissao.module] = {
          module: permissao.module,
          permissoes: []
        };
      }
      grupos[permissao.module].permissoes.push(permissao);
    });

    this.permissoesAgrupadas = Object.values(grupos);
  }

  temPermissao(permissaoId: number): boolean {
    // Mock: verificar se a role tem a permissão
    return this.roleSelecionada && this.roleSelecionada.id === 1; // Administrador tem todas
  }

  togglePermissao(permissaoId: number): void {
    console.log('Alternar permissão:', permissaoId);
    // Implementar lógica real aqui
  }

  salvarPermissoes(): void {
    console.log('Salvar permissões para:', this.roleSelecionada);
    // Implementar lógica real aqui
  }
}
