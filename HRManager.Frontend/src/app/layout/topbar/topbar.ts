import { Component, ElementRef, EventEmitter, HostListener, inject, Output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { Notificacao } from '../../interfaces/notificacao';
import { NotificacaoService } from '../../services/notificacao.service';
import { UserDetails } from '../../interfaces/userDetailsDto';

@Component({
  selector: 'app-topbar',
  imports: [CommonModule, RouterLink],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css',
})
export class Topbar {
  // 1. Criar o emissor de evento
  @Output() toggleSidebarEvent = new EventEmitter<void>();

  // private authService = inject(AuthService);
  private router = inject(Router);

  public authService = inject(AuthService); // Para o botão de logout e nome
  public notifService = inject(NotificacaoService);
  private elementRef = inject(ElementRef); // Para detetar cliques fora

  public notificacoes: Notificacao[] = [];
  public isDropdownOpen = false;
  public totalNaoLidas = 0;

  // --- Estados dos Dropdowns ---
  public isNotifDropdownOpen = false; // Renomeado para clareza
  public isUserDropdownOpen = false;  // NOVO

  // --- Dados do Utilizador ---
  public userEmail: string | null = null;
  public userInitial: string = ''; // Começa vazio
  public userRoleAndInst: string = ''; // Texto extra (ex: "GestorRH - Empresa X")

  ngOnInit() {
    this.carregarNotificacoes();
    this.notifService.totalNaoLidas.subscribe(num => this.totalNaoLidas = num);

    // Obter dados do utilizador (do token ou localStorage)
    // Nota: Idealmente o AuthService teria um método getUserDetails(), mas usamos o que temos
    // *** LÓGICA REAL DE UTILIZADOR ***
    this.authService.getUserDetails().subscribe({
      next: (user: UserDetails) => {
        this.userEmail = user.email;
        this.userInitial = user.email.charAt(0).toUpperCase();

        // Construir subtítulo (ex: "GestorMaster" ou "GestorRH - TechCorp")
        this.userRoleAndInst = user.role;
        if (user.nomeInstituicao) {
          this.userRoleAndInst += ` • ${user.nomeInstituicao}`;
        }
      },
      error: (err) => {
        console.error('Erro ao carregar perfil:', err);
        // Se falhar (ex: token inválido), podemos forçar logout ou mostrar padrão
        this.userEmail = 'Convidado';
      }
    });
    // (Opcional) Polling simples a cada 60 segundos
    setInterval(() => this.carregarNotificacoes(), 60000);
  }

  carregarNotificacoes() {
    this.notifService.getMinhas().subscribe(data => {
      this.notificacoes = data;
    });
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  toggleNotifDropdown() {
    this.isNotifDropdownOpen = !this.isNotifDropdownOpen;
    if (this.isNotifDropdownOpen) this.isUserDropdownOpen = false; // Fecha o outro
  }

  toggleUserDropdown() {
    this.isUserDropdownOpen = !this.isUserDropdownOpen;
    if (this.isUserDropdownOpen) this.isNotifDropdownOpen = false; // Fecha o outro
  }

  lerNotificacao(notif: Notificacao) {
    this.notifService.marcarLida(notif.id).subscribe(() => {
        this.carregarNotificacoes(); // Atualiza a lista
    });
    this.isDropdownOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // 2. Criar a função que emite o evento
  onToggleSidebar(): void {
    this.toggleSidebarEvent.emit();
  }

  // Fecha os dropdowns se clicar fora deles
  @HostListener('document:click', ['$event'])
  clickOut(event: any) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isNotifDropdownOpen = false;
      this.isUserDropdownOpen = false;
    }
  }
}
