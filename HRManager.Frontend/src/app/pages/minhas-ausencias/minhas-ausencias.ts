import { Component, inject, OnInit } from '@angular/core';
import { AusenciaDto } from '../../interfaces/ausenciaDto';
import { CriarAusenciaRequest } from '../../interfaces/criarAusenciaRequest';
import { AusenciaService } from '../../services/ausencia.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AusenciaSaldoDto } from '../../interfaces/ausenciaSaldoDto';

@Component({
  selector: 'app-minhas-ausencias',
  imports: [FormsModule, CommonModule],
  templateUrl: './minhas-ausencias.html',
  styleUrl: './minhas-ausencias.css',
})
export class MinhasAusencias implements OnInit {
  // Adicione esta constante ou propriedade com o URL base da sua API
  // (Idealmente viria do environment, mas para agora hardcoded serve)
  private readonly API_BASE_URL = 'https://localhost:7234';

  private ausenciaService = inject(AusenciaService);

  public listaAusencias: AusenciaDto[] = [];
  public dadosFormulario: CriarAusenciaRequest;

  public isModalAberto: boolean = false;

  // Feedback
  public feedbackMessage: string | null = null;
  public isError: boolean = false;
  public feedbackModal: string | null = null;
  public isErrorModal: boolean = false;

  // *** 1. PROPRIEDADE DE SALDO ***
  public saldoInfo: AusenciaSaldoDto | null = null;

  // Variável para guardar o ficheiro selecionado temporariamente
  public ficheiroSelecionado: File | null = null;

  constructor() {
    this.dadosFormulario = this.criarFormularioVazio();
  }

  ngOnInit(): void {
    this.carregarAusencias();
    this.carregarSaldo();
  }

  carregarAusencias(): void {
    this.ausenciaService.getAusencias().subscribe({
      next: (data) => {
        this.listaAusencias = data;
      },
      error: (err) => {
        console.error(err);
        this.mostrarFeedback('Erro ao carregar histórico.', true);
      }
    });
  }

  // *** 3. NOVO MÉTODO ***
  carregarSaldo(): void {
    this.ausenciaService.getSaldo().subscribe({
      next: (data) => this.saldoInfo = data,
      error: (err) => console.error('Erro ao carregar saldo', err)
    });
  }

  // Método disparado quando o utilizador escolhe um ficheiro
  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.ficheiroSelecionado = file;
    }
  }

  onSubmit(): void {
    this.limparFeedback(true);

    // Validação básica de datas
    if (this.dadosFormulario.dataInicio > this.dadosFormulario.dataFim) {
      this.mostrarFeedback('A data de fim deve ser superior à de início.', true, true);
      return;
    }
    // Adicionar o ficheiro ao objeto antes de enviar
    if (this.ficheiroSelecionado) {
      this.dadosFormulario.documento = this.ficheiroSelecionado;
    }

    this.ausenciaService.solicitarAusencia(this.dadosFormulario).subscribe({
      next: (res) => {
        this.mostrarFeedback('Pedido submetido com sucesso!', false);
        this.carregarAusencias();
        this.carregarSaldo(); // *** 4. Atualizar saldo após novo pedido ***
        this.fecharModal();
      },
      error: (err) => {
        const msg = err.error?.message || 'Erro ao submeter pedido.';
        this.mostrarFeedback(msg, true, true);
      }
    });
  }

  // --- Controlo do Modal ---
  abrirModal(): void {
    this.dadosFormulario = this.criarFormularioVazio();
    this.limparFeedback(true);
    this.isModalAberto = true;
  }

  fecharModal(): void {
    this.isModalAberto = false;
    this.ficheiroSelecionado = null; // Limpar o ficheiro selecionado ao fechar
  }

  // --- Auxiliares ---
  private criarFormularioVazio(): CriarAusenciaRequest {
    return {
      tipo: 'Ferias', // Valor por defeito
      dataInicio: new Date().toISOString().split('T')[0],
      dataFim: new Date().toISOString().split('T')[0],
      motivo: ''
    };
  }

  private mostrarFeedback(msg: string, isError: boolean, noModal: boolean = false): void {
    if (noModal) {
      this.feedbackModal = msg;
      this.isErrorModal = isError;
    } else {
      this.feedbackMessage = msg;
      this.isError = isError;
    }
  }

  private limparFeedback(noModal: boolean = false): void {
    if (noModal) {
      this.feedbackModal = null;
      this.isErrorModal = false;
    } else {
      this.feedbackMessage = null;
      this.isError = false;
    }
  }

  /**
   * Gera o link completo para o documento
   */
  getDocumentoUrl(caminho: string): string {
    return `${this.API_BASE_URL}/${caminho}`;
  }
}
