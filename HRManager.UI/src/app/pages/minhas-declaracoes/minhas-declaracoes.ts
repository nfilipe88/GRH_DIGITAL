import { Component, inject, OnInit } from '@angular/core';
import { CriarPedidoDeclaracaoRequest } from '../../interfaces/criarPedidoDeclarcaoRequest';
import { PedidoDeclaracaoDto } from '../../interfaces/pedidoDeclaracao';
import { DeclaracaoService } from '../../services/declaracao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment.prod';

@Component({
  selector: 'app-minhas-declaracoes',
  imports: [FormsModule, CommonModule],
  templateUrl: './minhas-declaracoes.html',
  styleUrl: './minhas-declaracoes.css',
})
export class MinhasDeclaracoes implements OnInit {
  private declaracaoService = inject(DeclaracaoService);

  // Variáveis alinhadas com o HTML
  public listaPedidos: PedidoDeclaracaoDto[] = [];
  public dadosFormulario: CriarPedidoDeclaracaoRequest = { tipo: 'FinsBancarios', observacoes: '' };

  public isModalAberto = false;
  public feedbackMessage: string | null = null;
  public loading = false;

  ngOnInit() {
    this.carregarPedidos();
  }

  carregarPedidos() {
    this.declaracaoService.getMinhas().subscribe({
      next: (data: PedidoDeclaracaoDto[]) => {
        this.listaPedidos = data;
      },
      error: (err: any) => console.error(err)
    });
  }

  onSubmit() {
    if (!this.dadosFormulario.tipo) return;

    this.loading = true;
    this.declaracaoService.solicitar(this.dadosFormulario).subscribe({
      next: () => {
        this.feedbackMessage = 'Pedido enviado com sucesso!';
        this.loading = false;
        this.isModalAberto = false;
        this.carregarPedidos();
        this.dadosFormulario = { tipo: 'FinsBancarios', observacoes: '' }; // Reset
        setTimeout(() => this.feedbackMessage = null, 3000);
      },
      error: (err: any) => {
        alert('Erro ao enviar pedido.');
        this.loading = false;
      }
    });
  }

  // Helper para o HTML
  getDocumentoUrl(caminho: string): string {
    return `${environment.apiUrl}/${caminho}`; // Ajusta se o caminho vier relativo ou absoluto
  }
}
