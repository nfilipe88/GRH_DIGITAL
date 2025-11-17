import { Component, inject, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { DashboardStats } from '../../interfaces/dashboard-stats';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
private dashboardService = inject(DashboardService);

  // Propriedade para guardar os dados
  // public stats: DashboardStats | null = null;
  // public isLoading: boolean = true;
  // public errorMessage: string | null = null;

  // ngOnInit(): void {
  //   this.carregarStats();
  // }

  // carregarStats(): void {
  //   this.isLoading = true;
  //   this.errorMessage = null;

  //   this.dashboardService.getStats().subscribe({
  //     next: (data) => {
  //       this.stats = data;
  //       this.isLoading = false;
  //     },
  //     error: (err) => {
  //       this.errorMessage = "Não foi possível carregar as estatísticas.";
  //       this.isLoading = false;
  //       console.error(err);
  //     }
  //   });
  // }

  // 2. Criar um Observable para os stats
  public stats$!: Observable<DashboardStats>;

  ngOnInit(): void {
    // 3. Chamar o serviço para carregar os dados
    this.stats$ = this.dashboardService.getStats();
  }
}
