import { Component, inject, OnInit } from '@angular/core';
import { AusenciaDto } from '../../interfaces/ausenciaDto';
import { AusenciaService } from '../../services/ausencia.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-gestao-ausencias',
  imports: [FormsModule, CommonModule],
  templateUrl: './gestao-ausencias.html',
  styleUrl: './gestao-ausencias.css',
})
export class GestaoAusencias implements OnInit {

  private ausenciaService = inject(AusenciaService);

  public listaAusencias: AusenciaDto[] = [];

  // --- Controlo do Modal de Rejeição ---
  public isModalRejeicaoAberto: boolean = false;
  public idAusenciaEmAnalise: number | null = null;
  public motivoRejeicao: string = '';

  // --- Feedback ---
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

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

  confirmarRejeicao(): void {
    if (!this.idAusenciaEmAnalise) return;
    if (!this.motivoRejeicao.trim()) {
      alert('Por favor, indique o motivo da rejeição.');
      return;
    }

    this.ausenciaService.responderAusencia(this.idAusenciaEmAnalise, {
      aprovado: false,
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
    this.idAusenciaEmAnalise = null;
  }

  private mostrarFeedback(msg: string, isError: boolean): void {
    this.feedbackMessage = msg;
    this.isError = isError;
    window.scrollTo({ top: 0, behavior: 'smooth' });

    // Limpa a mensagem após 5 segundos
    setTimeout(() => this.feedbackMessage = null, 5000);
  }

}
