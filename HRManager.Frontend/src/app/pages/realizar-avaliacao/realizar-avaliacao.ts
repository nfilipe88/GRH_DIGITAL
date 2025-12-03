import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SubmeterAvaliacaoRequest } from '../../interfaces/SubmeterAvaliacaoRequest';
import { AvaliacaoService } from '../../services/avaliacao.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Avaliacao } from '../../interfaces/Avaliacao';

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

  ngOnInit() {
    // Pegar o ID da URL (ex: /avaliacoes/123)
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.carregarAvaliacao(id);
    }
  }

  carregarAvaliacao(id: number) {
    this.loading = true;
    this.avaliacaoService.getAvaliacao(id).subscribe({
      next: (data) => {
        this.avaliacao = data;
        // Se já tiver comentário final guardado (rascunho), carrega-o
        // this.comentarioFinal = data.comentarioFinalGestor || '';
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar', err);
        this.loading = false;
      }
    });
  }

  // Calcula a média em tempo real baseada nas notas preenchidas
  get mediaAtual(): number {
    if (!this.avaliacao || !this.avaliacao.itens) return 0;

    const notas = this.avaliacao.itens
      .map(i => i.notaGestor)
      .filter(n => n !== null && n > 0) as number[];

    if (notas.length === 0) return 0;

    const soma = notas.reduce((a, b) => a + b, 0);
    return soma / notas.length;
  }

  // Verifica se tudo foi preenchido para permitir finalizar
  get podeFinalizar(): boolean {
    if (!this.avaliacao) return false;
    // Todas as notas devem ser > 0 e comentários obrigatórios (opcional)
    return this.avaliacao.itens.every(i => i.notaGestor !== null && i.notaGestor > 0);
  }

  salvar(finalizar: boolean) {
    if (!this.avaliacao) return;

    if (finalizar && !confirm('Tem a certeza? Após finalizar não poderá alterar as notas.')) {
      return;
    }

    const request: SubmeterAvaliacaoRequest = {
      finalizar: finalizar,
      comentarioFinal: this.comentarioFinal,
      respostas: this.avaliacao.itens.map(i => ({
        itemId: i.id,
        nota: i.notaGestor || 0,
        comentario: i.comentario || ''
      }))
    };

    this.avaliacaoService.submeterAvaliacao(this.avaliacao.id, request).subscribe({
      next: () => {
        alert(finalizar ? 'Avaliação concluída com sucesso! 🎉' : 'Rascunho guardado.');
        this.router.navigate(['/dashboard']); // Ou voltar à lista
      },
      error: (err) => alert('Erro ao salvar: ' + err.message)
    });
  }
}
