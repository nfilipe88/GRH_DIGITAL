import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Competencia, CicloAvaliacao, CriarCompetenciaRequest, CriarCicloRequest } from '../../interfaces/configuracao-avaliacao';
import { AvaliacaoService } from '../../services/avaliacao.service';

@Component({
  selector: 'app-configuracao-avaliacao',
  imports: [CommonModule, FormsModule],
  templateUrl: './configuracao-avaliacao.html',
  styleUrl: './configuracao-avaliacao.css',
})
export class ConfiguracaoAvaliacao implements OnInit {
  private avaliacaoService = inject(AvaliacaoService);

  // Estado da UI
  activeTab: 'competencias' | 'ciclos' = 'competencias';
  loading = false;

  // Dados
  listaCompetencias: Competencia[] = [];
  listaCiclos: CicloAvaliacao[] = [];

  // Formulários
  novaCompetencia: CriarCompetenciaRequest = { nome: '', descricao: '', tipo: 0 };
  novoCiclo: CriarCicloRequest = {
    nome: '',
    dataInicio: new Date().toISOString().split('T')[0],
    dataFim: ''
  };

  ngOnInit() {
    this.carregarCompetencias();
    this.carregarCiclos();
  }

  // --- CARREGAMENTO ---
  carregarCompetencias() {
    this.avaliacaoService.getCompetencias().subscribe(data => this.listaCompetencias = data);
  }

  carregarCiclos() {
    this.avaliacaoService.getCiclos().subscribe(data => this.listaCiclos = data);
  }

  // --- AÇÕES COMPETÊNCIAS ---
  adicionarCompetencia() {
    if (!this.novaCompetencia.nome) return;

    this.loading = true;
    this.avaliacaoService.criarCompetencia(this.novaCompetencia).subscribe({
      next: (res) => {
        this.listaCompetencias.push(res);
        this.novaCompetencia = { nome: '', descricao: '', tipo: 0 }; // Reset
        this.loading = false;
        alert('Competência adicionada!');
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  // --- AÇÕES CICLOS ---
  adicionarCiclo() {
    if (!this.novoCiclo.nome || !this.novoCiclo.dataFim) {
      alert('Preencha o nome e a data de fim.');
      return;
    }

    this.loading = true;
    this.avaliacaoService.criarCiclo(this.novoCiclo).subscribe({
      next: (res) => {
        this.listaCiclos.unshift(res); // Adiciona no topo
        this.novoCiclo.nome = ''; // Reset parcial
        this.loading = false;
        alert('Ciclo criado com sucesso!');
      },
      error: (err) => {
        alert(err.error?.message || 'Erro ao criar ciclo.');
        this.loading = false;
      }
    });
  }
}
