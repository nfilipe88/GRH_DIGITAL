import { Component, inject, OnInit } from '@angular/core';
import { CriarCertificacaoRequest } from '../../interfaces/criarCertificacaoRequest';
import { CriarHabilitacaoRequest } from '../../interfaces/criarHabilitacaoRequest';
import { PerfilDto } from '../../interfaces/perfilDto';
import { PerfilService } from '../../services/perfil.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AtualizarDadosPessoaisRequest } from '../../interfaces/atualizarDadosPessoaisRequest';

@Component({
  selector: 'app-perfil',
  imports: [FormsModule, CommonModule],
  templateUrl: './perfil.html',
  styleUrl: './perfil.css',
})
export class Perfil implements OnInit {

  private perfilService = inject(PerfilService);
  private readonly API_BASE_URL = 'https://localhost:7234'; // Para links de documentos

  public perfil: PerfilDto | null = null;

  // --- Estado dos Modais ---
  public isModalHabilitacaoAberto = false;
  public isModalCertificacaoAberto = false;

  // --- Formulários ---
  public formHabilitacao: CriarHabilitacaoRequest;
  public formCertificacao: CriarCertificacaoRequest;
  public ficheiroSelecionado: File | null = null;

  // --- Feedback ---
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  // --- Estado de Edição de Dados Pessoais ---
  public isEditingDados = false;
  public formDadosPessoais: AtualizarDadosPessoaisRequest = {};

  constructor() {
    this.formHabilitacao = this.resetHabilitacao();
    this.formCertificacao = this.resetCertificacao();
  }

  ngOnInit(): void {
    this.carregarPerfil();
  }

  carregarPerfil(): void {
    // Sem ID = carrega o perfil do utilizador logado
    this.perfilService.getPerfil().subscribe({
      next: (data) => this.perfil = data,
      error: (err) => console.error('Erro ao carregar perfil', err)
    });
  }

  // --- Ações de Habilitação ---

  abrirModalHabilitacao(): void {
    this.formHabilitacao = this.resetHabilitacao();
    this.ficheiroSelecionado = null;
    this.isModalHabilitacaoAberto = true;
  }

  submitHabilitacao(): void {
    if (this.ficheiroSelecionado) {
      this.formHabilitacao.documento = this.ficheiroSelecionado;
    }

    this.perfilService.addHabilitacao(this.formHabilitacao).subscribe({
      next: () => {
        this.mostrarFeedback('Habilitação adicionada com sucesso!', false);
        this.carregarPerfil();
        this.isModalHabilitacaoAberto = false;
      },
      error: (err) => this.mostrarFeedback('Erro ao adicionar habilitação.', true)
    });
  }

  deleteHabilitacao(id: number): void {
    if(!confirm('Tem a certeza?')) return;
    this.perfilService.deleteHabilitacao(id).subscribe({
      next: () => this.carregarPerfil(),
      error: (err) => alert('Erro ao remover.')
    });
  }

  // --- Ações de Certificação ---

  abrirModalCertificacao(): void {
    this.formCertificacao = this.resetCertificacao();
    this.ficheiroSelecionado = null;
    this.isModalCertificacaoAberto = true;
  }

  submitCertificacao(): void {
    if (this.ficheiroSelecionado) {
      this.formCertificacao.documento = this.ficheiroSelecionado;
    }

    this.perfilService.addCertificacao(this.formCertificacao).subscribe({
      next: () => {
        this.mostrarFeedback('Certificação adicionada com sucesso!', false);
        this.carregarPerfil();
        this.isModalCertificacaoAberto = false;
      },
      error: (err) => this.mostrarFeedback('Erro ao adicionar certificação.', true)
    });
  }

  deleteCertificacao(id: number): void {
    if(!confirm('Tem a certeza?')) return;
    this.perfilService.deleteCertificacao(id).subscribe({
      next: () => this.carregarPerfil(),
      error: (err) => alert('Erro ao remover.')
    });
  }

  // *** MÉTODOS PARA DADOS PESSOAIS ***

  ativarEdicaoDados(): void {
    if (!this.perfil) return;
    // Copiar dados atuais para o formulário
    this.formDadosPessoais = {
      morada: this.perfil.morada,
      iban: this.perfil.iban
    };
    this.isEditingDados = true;
  }

  cancelarEdicaoDados(): void {
    this.isEditingDados = false;
    this.formDadosPessoais = {};
  }

  guardarDadosPessoais(): void {
    this.perfilService.updateDadosPessoais(this.formDadosPessoais).subscribe({
      next: () => {
        this.mostrarFeedback('Dados atualizados com sucesso!', false);
        this.carregarPerfil(); // Recarrega para ver as alterações
        this.isEditingDados = false;
      },
      error: (err) => this.mostrarFeedback('Erro ao atualizar dados.', true)
    });
  }

  // --- Auxiliares ---

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) this.ficheiroSelecionado = file;
  }

  getDocumentoUrl(caminho: string): string {
    return `${this.API_BASE_URL}/${caminho}`;
  }

  private resetHabilitacao(): CriarHabilitacaoRequest {
    return { grau: 'Licenciatura', curso: '', instituicaoEnsino: '', dataConclusao: '' };
  }

  private resetCertificacao(): CriarCertificacaoRequest {
    return { nomeCertificacao: '', entidadeEmissora: '', dataEmissao: '', dataValidade: '' };
  }

  private mostrarFeedback(msg: string, isError: boolean): void {
    this.feedbackMessage = msg;
    this.isError = isError;
    setTimeout(() => this.feedbackMessage = null, 4000);
  }
}
