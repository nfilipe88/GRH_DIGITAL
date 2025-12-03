import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { CommonModule } from '@angular/common';
import { NgxChartsModule, Color, ScaleType } from '@swimlane/ngx-charts';
import { DashboardStats } from '../../interfaces/dashboard-stats';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, NgxChartsModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',// CORREÇÃO 1: Mudar estratégia para OnPush para evitar o erro NG0100
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Dashboard implements OnInit {
private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef); // CORREÇÃO 2: Injetar ChangeDetectorRef

  stats: DashboardStats | null = null;

  // --- Configurações do Gráfico ---
  chartData: any[] = [];

  view: [number, number] = [0, 0];
  showLegend: boolean = true;
  showLabels: boolean = true;
  isDoughnut: boolean = false;

  colorScheme: Color = {
    name: 'customScheme',
    selectable: true,
    group: ScaleType.Ordinal,
    domain: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6']
  };

  ngOnInit() {
    this.carregarDados();
  }

  carregarDados() {
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.processarDadosGrafico(data.colaboradoresPorDepartamento);

        // CORREÇÃO 3: Avisar o Angular que os dados mudaram e ele deve atualizar a view
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Erro ao carregar dashboard', err)
    });
  }

  private processarDadosGrafico(dadosDict: { [key: string]: number }) {
    if (!dadosDict) return;

    this.chartData = Object.keys(dadosDict).map(key => ({
      name: key,
      value: dadosDict[key]
    }));
  }
}
