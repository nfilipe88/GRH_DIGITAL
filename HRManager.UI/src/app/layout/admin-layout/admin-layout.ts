import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, NgZone, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { Sidebar } from "../sidebar/sidebar";
import { Topbar } from "../topbar/topbar";

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, Sidebar, Topbar],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit {
  // 2. Injetar os serviços. Tornamos o authService público
  //    para que o template (app.html) o possa aceder.
  public authService = inject(AuthService);
  private router = inject(Router);
  private zone = inject(NgZone); // 3. Injetar NgZone para performance

  private inactivityTimer: any;
  private readonly INACTIVITY_TIMEOUT_MS = 1800000; // 30 minutos (30 * 60 * 1000)
  // 1. A nossa variável de estado
  public isSidebarOpen = true;

  ngOnInit(): void {
    // 4. Iniciar o temporizador assim que o componente arranca
    this.resetInactivityTimer();
  }

  // 5. O HostListener "ouve" eventos globais
  @HostListener('window:mousemove')
  @HostListener('window:keydown')
  @HostListener('window:click')
  onUserActivity() {
    this.resetInactivityTimer();
  }

  /**
   * Reinicia o temporizador de inatividade.
   */
  private resetInactivityTimer(): void {
    // Só faz sentido ter um temporizador se o utilizador estiver logado
    if (!this.authService.isLoggedIn()) {
      return;
    }

    // Usamos NgZone.runOutsideAngular para que estas ações
    // não disparem a deteção de alterações do Angular a cada clique,
    // o que melhora a performance.
    this.zone.runOutsideAngular(() => {
      // Limpa o temporizador antigo
      if (this.inactivityTimer) {
        clearTimeout(this.inactivityTimer);
      }

      // Define um novo temporizador
      this.inactivityTimer = setTimeout(() => {
        // Quando o temporizador expira, voltamos "para dentro" do Angular
        // para executar o logout e o redirecionamento.
        this.zone.run(() => {
          if (this.authService.isLoggedIn()) {
            console.log('Sessão expirada devido a inatividade.');
            this.logout(true); // Faz logout e informa o motivo
          }
        });
      }, this.INACTIVITY_TIMEOUT_MS);
    });
  }

  /**
   * Método de logout, agora com um parâmetro opcional
   */
  public logout(isInactive: boolean = false): void {
    this.authService.logout();

    if (isInactive) {
      // Se foi por inatividade, podemos adicionar uma mensagem na URL
      this.router.navigate(['/login'], { queryParams: { reason: 'inactive' } });
    } else {
      this.router.navigate(['/login']);
    }
  }


  // 2. A função que troca o estado
  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
}
