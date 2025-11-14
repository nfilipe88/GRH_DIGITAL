import { Component, inject, OnInit } from '@angular/core';
import { AuthService, RegisterRequest } from '../../services/auth.service';
import { InstituicaoService, Instituicao } from '../../services/instituicao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-gestao-utilizadores',
  imports: [FormsModule, CommonModule],
  templateUrl: './gestao-utilizadores.html',
  styleUrl: './gestao-utilizadores.css',
})
export class GestaoUtilizadores implements OnInit {
  // --- Serviços ---
  private authService = inject(AuthService);
  private instituicaoService = inject(InstituicaoService);

  // --- Estado do Formulário ---
  public dadosFormulario: RegisterRequest;
  public listaInstituicoes: Instituicao[] = [];

  // --- Feedback ---
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  constructor() {
    this.dadosFormulario = this.criarFormularioVazio();
  }

  ngOnInit(): void {
    this.carregarInstituicoes();
  }

  /**
   * Busca instituições ativas para o dropdown
   */
  carregarInstituicoes(): void {
    this.instituicaoService.getInstituicoes().subscribe({
      next: (data) => {
        this.listaInstituicoes = data.filter(inst => inst.isAtiva);
      },
      error: (err) => {
        this.mostrarFeedback('Erro ao carregar lista de instituições.', true);
      }
    });
  }

  /**
   * Chamado ao submeter o formulário
   */
  onSubmit(): void {
    this.limparFeedback();

    // Ajuste final: InstituicaoId só é relevante se for GestorRH
    if (this.dadosFormulario.role !== 'GestorRH') {
      this.dadosFormulario.instituicaoId = null;
    }

    this.authService.register(this.dadosFormulario).subscribe({
      next: (response) => {
        this.mostrarFeedback(response.message || 'Utilizador registado com sucesso!', false);
        this.dadosFormulario = this.criarFormularioVazio();
        // No futuro, vamos recarregar uma lista de utilizadores aqui
      },
      error: (err) => {
        const msg = err.error?.message || 'Erro ao registar utilizador.';
        this.mostrarFeedback(msg, true);
      }
    });
  }

  // --- Métodos Auxiliares ---

  private criarFormularioVazio(): RegisterRequest {
    return {
      email: '',
      password: '',
      role: 'GestorRH', // 'GestorRH' como padrão
      instituicaoId: null
    };
  }

  private mostrarFeedback(mensagem: string, ehErro: boolean): void {
    this.feedbackMessage = mensagem;
    this.isError = ehErro;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private limparFeedback(): void {
    this.feedbackMessage = null;
    this.isError = false;
  }
}
