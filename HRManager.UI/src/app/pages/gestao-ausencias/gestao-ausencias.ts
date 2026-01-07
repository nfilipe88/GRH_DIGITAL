import { Component, inject, OnInit } from '@angular/core';
import { AusenciaDto } from '../../interfaces/ausenciaDto';
import { AusenciaService } from '../../services/ausencia.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-gestao-ausencias',
  imports: [FormsModule, CommonModule],
  templateUrl: './gestao-ausencias.html',
  styleUrl: './gestao-ausencias.css',
})
export class GestaoAusencias implements OnInit {
  private readonly API_BASE_URL = 'https://localhost:7234';

  private ausenciaService = inject(AusenciaService);
  public listaAusencias: AusenciaDto[] = [];
  modalAberto= false;

  // --- Controlo do Modal de Rejeição ---
  public isModalRejeicaoAberto: boolean = false;
  public idAusenciaEmAnalise: string = '';
  public motivoRejeicao: string = '';

  respostaAprovada: boolean = true;
  comentarioGestor: string = '';

  // --- Feedback ---
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  // --- Filtros para Relatório ---
  public filtroMes: number;
  public filtroAno: number;
  public isExporting = false;

  constructor() {
      // Definir mês/ano atual como padrão
      const hoje = new Date();
      this.filtroMes = hoje.getMonth() + 1; // JS conta meses de 0 a 11
      this.filtroAno = hoje.getFullYear();
  }

  ngOnInit(): void {
    this.carregarAusencias();
  }

  carregarAusencias(): void {
    this.ausenciaService.getAusencias().subscribe({
      next: (data) => {
        this.listaAusencias = data;
      },
      error: (err) => {
        console.error(err);
        this.mostrarFeedback('Erro ao carregar lista de ausências.', true);
      }
    });
  }

  /**
   * Ação de Aprovar (Imediata)
   */
  aprovar(ausencia: AusenciaDto): void {
    if (!confirm(`Confirma a aprovação das férias de ${ausencia.nomeColaborador}?`)) {
      return;
    }

    this.ausenciaService.responderAusencia(ausencia.id, { aprovado: true }).subscribe({
      next: () => {
        this.mostrarFeedback(`Ausência de ${ausencia.nomeColaborador} aprovada.`, false);
        this.carregarAusencias();
      },
      error: (err) => {
        this.mostrarFeedback(err.error?.message || 'Erro ao aprovar.', true);
      }
    });
  }

  /**
   * Ação de Rejeitar (Abre Modal)
   */
  abrirModalRejeicao(ausencia: AusenciaDto): void {
    this.idAusenciaEmAnalise = ausencia.id;
    this.motivoRejeicao = ''; // Limpa o campo
    this.isModalRejeicaoAberto = true;
  }

  abrirModalResposta(ausencia: AusenciaDto) {
    this.idAusenciaEmAnalise = ausencia.id;
    this.respostaAprovada = true; // Reset ao valor padrão
    this.comentarioGestor = '';   // Reset ao comentário
    this.modalAberto = true;
  }

  confirmarRejeicao(): void {
    if (!this.idAusenciaEmAnalise) return;
    if (!this.motivoRejeicao.trim()) {
      alert('Por favor, indique o motivo da rejeição.');
      return;
    }

    this.ausenciaService.responderAusencia(this.idAusenciaEmAnalise, {
      aprovado: this.respostaAprovada,
      comentario: this.motivoRejeicao
    }).subscribe({
      next: () => {
        this.mostrarFeedback('Solicitação rejeitada com sucesso.', false);
        this.fecharModal();
        this.carregarAusencias();
      },
      error: (err) => {
        alert(err.error?.message || 'Erro ao rejeitar.');
      }
    });
  }

  fecharModal(): void {
    this.isModalRejeicaoAberto = false;
    this.idAusenciaEmAnalise = '';
  }

  private mostrarFeedback(msg: string, isError: boolean): void {
    this.feedbackMessage = msg;
    this.isError = isError;
    window.scrollTo({ top: 0, behavior: 'smooth' });

    // Limpa a mensagem após 5 segundos
    setTimeout(() => this.feedbackMessage = null, 5000);
  }

  /**
   * Gera o link completo para o documento
   */
  getDocumentoUrl(caminho: string): string {
    // 3. USAR environment.baseUrl
    return `${environment.baseUrl}/${caminho}`;
  }

  /**
   * Ação de Exportar
   */
  exportarExcel(): void {
    this.isExporting = true;
    this.ausenciaService.downloadRelatorioExcel(this.filtroMes, this.filtroAno).subscribe({
      next: (blob) => {
        // Criar um link temporário para forçar o download no browser
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Relatorio_Ausencias_${this.filtroMes}_${this.filtroAno}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);

        this.isExporting = false;
        this.mostrarFeedback('Relatório gerado com sucesso.', false);
      },
      error: (err) => {
        console.error(err);
        this.isExporting = false;
        this.mostrarFeedback('Erro ao gerar relatório.', true);
      }
    });
  }
}
