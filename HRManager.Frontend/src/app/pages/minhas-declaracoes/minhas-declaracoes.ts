import { Component, inject, OnInit } from '@angular/core';
import { CriarPedidoDeclaracaoRequest } from '../../interfaces/criarPedidoDeclarcaoRequest';
import { PedidoDeclaracaoDto } from '../../interfaces/pedidoDeclaracao';
import { DeclaracaoService } from '../../services/declaracao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-minhas-declaracoes',
  imports: [FormsModule, CommonModule],
  templateUrl: './minhas-declaracoes.html',
  styleUrl: './minhas-declaracoes.css',
})
export class MinhasDeclaracoes implements OnInit {
  private declaracaoService = inject(DeclaracaoService);
  private readonly API_BASE_URL = 'https://localhost:7234';

  public listaPedidos: PedidoDeclaracaoDto[] = [];
  public dadosFormulario: CriarPedidoDeclaracaoRequest = { tipo: 'FinsBancarios', observacoes: '' };

  public isModalAberto = false;
  public feedbackMessage: string | null = null;

  ngOnInit() {
    this.carregarPedidos();
  }

  carregarPedidos() {
    this.declaracaoService.getPedidos().subscribe({
      next: (data) => this.listaPedidos = data,
      error: (err) => console.error(err)
    });
  }

  onSubmit() {
    this.declaracaoService.solicitar(this.dadosFormulario).subscribe({
      next: () => {
        this.feedbackMessage = 'Pedido enviado ao RH.';
        this.carregarPedidos();
        this.isModalAberto = false;
        this.dadosFormulario = { tipo: 'FinsBancarios', observacoes: '' };
        setTimeout(() => this.feedbackMessage = null, 3000);
      },
      error: (err) => alert(err.error?.message || 'Erro ao pedir.')
    });
  }

  getDocumentoUrl(caminho: string): string {
    return `${this.API_BASE_URL}/${caminho}`;
  }
}
