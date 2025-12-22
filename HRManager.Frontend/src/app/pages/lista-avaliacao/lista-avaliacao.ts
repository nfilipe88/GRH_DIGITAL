import { AvaliacaoDto } from './../../interfaces/AvaliacaoDto';
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { AvaliacaoService } from '../../services/avaliacao.service';

@Component({
  selector: 'app-lista-avaliacao',
  imports: [CommonModule, RouterModule],
  templateUrl: './lista-avaliacao.html',
  styleUrl: './lista-avaliacao.css',
})
export class ListaAvaliacao implements OnInit {
  private avaliacaoService = inject(AvaliacaoService);
  private router = inject(Router);

  avaliacoes: AvaliacaoDto[] = [];
  loading = true;

  ngOnInit() {
    this.carregarLista();
  }

  carregarLista() {
    this.loading = true;
    this.avaliacaoService.getAvaliacoesEquipa().subscribe({
      next: (data) => {
        this.avaliacoes = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar avaliações', err);
        this.loading = false;
      }
    });
  }

  // Navega para a página de realização (aquela que criámos antes)
  iniciarAvaliacao(id: string) {
    this.router.navigate(['/avaliacoes/realizar', id]);
  }

  // Helper para classes CSS de estado
  getEstadoClass(estado: string): string {
    switch (estado) {
      case 'Concluida': return 'bg-green-100 text-green-700 border-green-200';
      case 'EmAndamento': return 'bg-blue-100 text-blue-700 border-blue-200';
      default: return 'bg-slate-100 text-slate-600 border-slate-200';
    }
  }

  getEstadoLabel(estado: string): string {
     // Traduz o Enum do C# para texto amigável se necessário
     return estado.replace(/([A-Z])/g, ' $1').trim();
  }
}
