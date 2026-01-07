import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { AusenciaDto } from '../../interfaces/ausenciaDto';
import { AusenciaService } from '../../services/ausencia.service';

@Component({
  selector: 'app-gestao-calendario',
  imports: [CommonModule],
  templateUrl: './gestao-calendario.html',
  styleUrl: './gestao-calendario.css',
})
export class GestaoCalendario implements OnInit {

  private ausenciaService = inject(AusenciaService);

  public listaAusencias: AusenciaDto[] = [];

  // --- Estado do Calendário ---
  public dataAtual: Date = new Date(); // O mês que estamos a ver
  public diasDoMes: Date[] = []; // A lista de dias para desenhar
  public diasVaziosInicio: number[] = []; // Espaços em branco antes do dia 1

  public diasDaSemana = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];

  ngOnInit(): void {
    this.carregarAusencias();
  }

  carregarAusencias(): void {
    this.ausenciaService.getAusencias().subscribe({
      next: (data) => {
        // Filtramos apenas as aprovadas e pendentes (ignoramos rejeitadas)
        this.listaAusencias = data.filter(a => a.estado !== 'Rejeitada' && a.estado !== 'Cancelada');
        this.gerarCalendario();
      },
      error: (err) => console.error('Erro ao carregar ausências', err)
    });
  }

  // --- Lógica do Calendário ---

  gerarCalendario(): void {
    const ano = this.dataAtual.getFullYear();
    const mes = this.dataAtual.getMonth();

    // 1. Primeiro dia do mês
    const primeiroDia = new Date(ano, mes, 1);
    // 2. Último dia do mês
    const ultimoDia = new Date(ano, mes + 1, 0);

    // 3. Determinar quantos espaços vazios precisamos no início (Dia da semana 0-6)
    const diaSemanaInicio = primeiroDia.getDay();
    this.diasVaziosInicio = Array(diaSemanaInicio).fill(0);

    // 4. Gerar a lista de dias reais
    this.diasDoMes = [];
    for (let d = 1; d <= ultimoDia.getDate(); d++) {
      this.diasDoMes.push(new Date(ano, mes, d));
    }
  }

  mudarMes(delta: number): void {
    // Avança ou recua os meses
    this.dataAtual.setMonth(this.dataAtual.getMonth() + delta);
    // Força a atualização da referência para o Angular detetar (opcional, mas boa prática)
    this.dataAtual = new Date(this.dataAtual);
    this.gerarCalendario();
  }

  // --- Lógica de Cruzamento de Dados ---

  /**
   * Verifica quais ausências coincidem com um dia específico
   */
  getAusenciasDoDia(dia: Date): AusenciaDto[] {
    // Convertemos o dia do calendário para o início do dia (00:00:00) para comparar
    const timeDia = dia.getTime();

    return this.listaAusencias.filter(a => {
      // Converter strings da API para Date
      const inicio = new Date(a.dataInicio);
      inicio.setHours(0,0,0,0);

      const fim = new Date(a.dataFim);
      fim.setHours(23,59,59,999);

      // Verificar se o dia está dentro do intervalo [Inicio, Fim]
      return timeDia >= inicio.getTime() && timeDia <= fim.getTime();
    });
  }
}
