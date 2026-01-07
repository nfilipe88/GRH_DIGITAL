import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts'; // Importação do gráfico
import { DashboardService } from '../../services/dashboard.service';
import { DashboardStats } from '../../interfaces/dashboard-stats';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, NgxChartsModule],
  templateUrl: './dashboard.html'
})
export class Dashboard implements OnInit {

  isLoading: boolean = true;
  stats: DashboardStats | null = null;

  // Variável para alimentar o gráfico (Lista de objetos {name, value})
  chartData: any[] = [];

  // Cores do gráfico (Estilo "Cool")
  colorScheme: Color = {
    name: 'cool',
    selectable: true,
    group: ScaleType.Ordinal,
    domain: ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899']
  };

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats() {
    this.isLoading = true;
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;

        // Se for gestor e tiver dados de departamentos, prepara o gráfico
        if (this.stats.isVisaoGestor && this.stats.colaboradoresPorDepartamento) {
          this.chartData = this.stats.colaboradoresPorDepartamento.map(d => ({
            name: d.nome,
            value: d.quantidade
          }));
        }
      },
      error: (err) => {
        console.error('Erro ao carregar dashboard', err);
        this.isLoading = false;
      }
    });
  }
}
