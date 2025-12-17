import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Avaliacao } from '../../interfaces/Avaliacao';
import { AvaliacaoService } from '../../services/avaliacao.service';
import { RealizarAutoAvaliacaoRequest } from '../../interfaces/realizarAutoAvaliacaoRequest';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-minhas-avaliacoes',
  imports: [CommonModule, FormsModule],
  templateUrl: './minhas-avaliacoes.html',
  styleUrl: './minhas-avaliacoes.css',
})
export class MinhasAvaliacoes implements OnInit {
  private avaliacaoService = inject(AvaliacaoService);

  avaliacoes: Avaliacao[] = [];
  avaliacaoSelecionada: Avaliacao | null = null;
  loading = true;
  isModalAberto = false;

  ngOnInit() {
    this.carregarDados();
  }

  carregarDados() {
    this.loading = true;
    this.avaliacaoService.getMinhasAvaliacoes().subscribe({
      next: (data) => {
        this.avaliacoes = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro', err);
        this.loading = false;
      }
    });
  }

  abrirAutoAvaliacao(av: Avaliacao) {
    // Só permite abrir se não estiver finalizada
    if (av.estado !== 'Finalizada') {
      this.avaliacaoSelecionada = { ...av }; // Cria uma cópia para edição
      this.isModalAberto = true;
    }
  }

  fecharModal() {
    this.isModalAberto = false;
    this.avaliacaoSelecionada = null;
  }

  salvarAutoAvaliacao(finalizar: boolean) {
    if (!this.avaliacaoSelecionada) return;

    if (finalizar && !confirm('Tem a certeza? Não poderá alterar após enviar.')) return;

    const request: RealizarAutoAvaliacaoRequest = {
      finalizar: finalizar,
      respostas: this.avaliacaoSelecionada.itens.map(i => ({
        itemId: i.id,
        nota: i.notaAutoAvaliacao || 0,
        comentario: i.justificativaColaborador || ''
      }))
    };

    this.avaliacaoService.submeterAutoAvaliacao(this.avaliacaoSelecionada.id, request).subscribe({
      next: () => {
        alert('Autoavaliação guardada com sucesso!');
        this.fecharModal();
        this.carregarDados();
      },
      error: (err) => alert('Erro ao guardar: ' + err.message)
    });
  }
}
