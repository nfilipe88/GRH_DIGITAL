import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AvaliacaoService } from '../../services/avaliacao.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Avaliacao } from '../../interfaces/Avaliacao';
import { RealizarAvaliacaoGestorRequest } from '../../interfaces/realizarAvaliacaoGestorRequest';

@Component({
  selector: 'app-realizar-avaliacao',
  imports: [CommonModule, FormsModule],
  templateUrl: './realizar-avaliacao.html',
  styleUrl: './realizar-avaliacao.css',
})
export class RealizarAvaliacao implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private avaliacaoService = inject(AvaliacaoService);

  avaliacao: Avaliacao | null = null;
  comentarioFinal: string = '';
  loading = true;

  // Escala de notas para o Select
  escalaNotas = [
    { valor: 1, texto: '1 - Insuficiente' },
    { valor: 2, texto: '2 - Precisa Melhorar' },
    { valor: 3, texto: '3 - Bom / Cumpre' },
    { valor: 4, texto: '4 - Muito Bom' },
    { valor: 5, texto: '5 - Excelente' }
  ];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') || '';
    if (id) {
      this.carregarAvaliacao(id);
    }
  }

  carregarAvaliacao(id: string) {
    this.loading = true;
    this.avaliacaoService.getAvaliacao(id).subscribe({
      next: (data) => {
        this.avaliacao = data;
        this.comentarioFinal = data.comentarioFinalGestor || '';
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar avaliação', err);
        alert('Erro ao carregar dados.');
        this.router.navigate(['/avaliacoes/equipa']);
        this.loading = false;
      }
    });
  }

  // --- Propriedades Computadas ---

  get mediaAtual(): number {
    if (!this.avaliacao || !this.avaliacao.itens || this.avaliacao.itens.length === 0) return 0;
    const itensComNota = this.avaliacao.itens.filter(i => i.notaGestor && i.notaGestor > 0);
    if (itensComNota.length === 0) return 0;
    const soma = itensComNota.reduce((acc, curr) => acc + (curr.notaGestor || 0), 0);
    return soma / itensComNota.length;
  }

  // CORREÇÃO: Adicionamos a propriedade que faltava para validar o botão
  get podeFinalizar(): boolean {
    if (!this.avaliacao || !this.avaliacao.itens) return false;
    // Só pode finalizar se TODOS os itens tiverem nota do gestor
    return this.avaliacao.itens.every(i => i.notaGestor !== null && i.notaGestor !== undefined && i.notaGestor > 0);
  }

  // --- Ações ---

  salvar(finalizar: boolean) {
    if (!this.avaliacao) return;

    if (finalizar) {
      if (!this.podeFinalizar) {
        alert('Por favor, atribua uma nota a todas as competências antes de finalizar.');
        return;
      }
      if (!confirm('Tem a certeza? A nota será calculada e enviada ao colaborador.')) {
        return;
      }
    }

    const request: RealizarAvaliacaoGestorRequest = {
      finalizar: finalizar,
      comentarioFinal: this.comentarioFinal,
      respostas: this.avaliacao.itens.map(i => ({
        itemId: i.id,
        nota: i.notaGestor || 0,
        comentario: i.justificativaGestor || ''
      }))
    };

    this.avaliacaoService.submeterAvaliacaoGestor(this.avaliacao.id, request).subscribe({
      next: () => {
        if (finalizar) {
          alert(`Concluído! Média Final: ${this.mediaAtual.toFixed(1)}`);
          this.router.navigate(['/avaliacoes/equipa']);
        } else {
          alert('Rascunho guardado.');
        }
      },
      error: (err) => {
        console.error(err);
        alert('Erro ao guardar.');
      }
    });
  }

  voltar() {
    this.router.navigate(['/avaliacoes/equipa']);
  }
}
