import { Component, inject, OnInit } from '@angular/core';
import { InstituicaoService } from '../../services/instituicao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService, RegisterRequest, UserListDto } from '../../../core/auth/auth.service';
import { Instituicao } from '../../interfaces/instituicao';

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
  public listaUtilizadores: UserListDto[] = [];

  // *** 1. ADICIONAR ESTADO DO MODAL ***
  public isModalAberto: boolean = false;

  // --- Feedback ---
  // Feedback da PÁGINA (para sucesso de registo, erros de lista)
  public feedbackMessage: string | null = null;
  public isError: boolean = false;
  // Feedback DO MODAL (para erros de validação do formulário)
  public feedbackModal: string | null = null;
  public isErrorModal: boolean = false;

  constructor() {
    this.dadosFormulario = this.criarFormularioVazio();
  }

  ngOnInit(): void {
    this.carregarInstituicoes();
    this.carregarUtilizadores();
  }

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

  carregarUtilizadores(): void {
    this.authService.getUsers().subscribe({
      next: (data) => {
        this.listaUtilizadores = data;
      },
      error: (err) => {
        console.error('Erro ao carregar utilizadores:', err);
        if (err.status === 403) {
          this.mostrarFeedback('Apenas o Gestor Master pode ver a lista de utilizadores.', true);
        }
      }
    });
  }

  /**
   * Chamado ao submeter o formulário
   */
  onSubmit(): void {
    this.limparFeedback(true); // Limpa feedback SÓ do modal

    if (this.dadosFormulario.role !== 'GestorRH') {
      this.dadosFormulario.instituicaoId = null;
    }
    // Validar se GestorRH tem instituição
    if(this.dadosFormulario.role === 'GestorRH' && !this.dadosFormulario.instituicaoId) {
        this.mostrarFeedback('Para registar um GestorRH, é obrigatório selecionar uma instituição.', true, true);
        return;
    }

    this.authService.register(this.dadosFormulario).subscribe({
      next: (response) => {
        // Sucesso: Mostra feedback na PÁGINA
        this.mostrarFeedback(response.message || 'Utilizador registado com sucesso!', false, false);
        this.carregarUtilizadores();
        this.fecharModal(); // Fecha o modal
      },
      error: (err) => {
        // Erro: Mostra feedback DENTRO do modal
        const msg = err.error?.message || 'Erro ao registar utilizador.';
        this.mostrarFeedback(msg, true, true);
      }
    });
  }

  // ---
  // *** 2. NOVOS MÉTODOS DE CONTROLO DO MODAL ***
  // ---

  /**
   * Abre o modal para criar um NOVO utilizador
   */
  public abrirModalNovo(): void {
    this.dadosFormulario = this.criarFormularioVazio();
    this.limparFeedback(true);
    this.isModalAberto = true;
  }

  /**
   * Fecha o modal
   */
  public fecharModal(): void {
    this.isModalAberto = false;
    this.limparFeedback(true);
  }

  // ---
  // *** 3. MÉTODOS AUXILIARES ATUALIZADOS ***
  // ---

  private criarFormularioVazio(): RegisterRequest {
    return {
      email: '',
      password: '',
      role: 'GestorRH',
      instituicaoId: null
    };
  }

  private mostrarFeedback(mensagem: string, ehErro: boolean, noModal: boolean = false): void {
    if (noModal) {
      this.feedbackModal = mensagem;
      this.isErrorModal = ehErro;
    } else {
      this.feedbackMessage = mensagem;
      this.isError = ehErro;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  private limparFeedback(apenasModal: boolean = false): void {
    if (apenasModal) {
      this.feedbackModal = null;
      this.isErrorModal = false;
    } else {
      this.feedbackMessage = null;
      this.isError = false;
      this.feedbackModal = null;
      this.isErrorModal = false;
    }
  }
}
