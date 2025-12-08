import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Avaliacao } from '../../interfaces/Avaliacao';
import { AvaliacaoService } from '../../services/avaliacao.service';

@Component({
  selector: 'app-minhas-avaliacoes',
  imports: [CommonModule],
  templateUrl: './minhas-avaliacoes.html',
  styleUrl: './minhas-avaliacoes.css',
})
export class MinhasAvaliacoes implements OnInit {
  private avaliacaoService = inject(AvaliacaoService);

  avaliacoes: Avaliacao[] = [];
  avaliacaoSelecionada: Avaliacao | null = null;
  loading = true;

  ngOnInit() {
    this.carregarDados();
  }

  carregarDados() {
    this.loading = true;
    this.avaliacaoService.getMinhasAvaliacoes().subscribe({
      next: (data) => {
        this.avaliacoes = data;
        // Se houver avaliações, seleciona a mais recente automaticamente
        if (data.length > 0) {
          this.avaliacaoSelecionada = data[0];
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar avaliações', err);
        this.loading = false;
      }
    });
  }

  selecionarAvaliacao(avaliacao: Avaliacao) {
    this.avaliacaoSelecionada = avaliacao;
  }

}
