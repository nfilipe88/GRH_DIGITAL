import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PedidoDeclaracaoDto } from '../../interfaces/pedidoDeclaracao';
import { DeclaracaoService } from '../../services/declaracao.service';

@Component({
  selector: 'app-emissao-declaracoes',
  imports: [FormsModule, CommonModule],
  templateUrl: './emissao-declaracoes.html',
  styleUrl: './emissao-declaracoes.css',
})
export class EmissaoDeclaracoes implements OnInit {

  private declaracaoService = inject(DeclaracaoService);
  private readonly API_BASE_URL = 'https://localhost:7234';

  public listaPedidos: PedidoDeclaracaoDto[] = [];

  // Estado do Modal de Upload
  public isModalUploadAberto = false;
  public pedidoEmAnalise: PedidoDeclaracaoDto | null = null;
  public ficheiroSelecionado: File | null = null;

  ngOnInit() {
    this.carregarPedidos();
  }

  carregarPedidos() {
    this.declaracaoService.getPedidos().subscribe({
      next: (data) => this.listaPedidos = data,
      error: (err) => console.error(err)
    });
  }

  // --- Ações ---

  abrirModalUpload(pedido: PedidoDeclaracaoDto) {
    this.pedidoEmAnalise = pedido;
    this.ficheiroSelecionado = null;
    this.isModalUploadAberto = true;
  }

  rejeitar(pedido: PedidoDeclaracaoDto) {
    if (!confirm(`Rejeitar o pedido de ${pedido.nomeColaborador}?`)) return;

    this.declaracaoService.resolver(pedido.id, null, true).subscribe({
      next: () => {
        alert('Pedido rejeitado.');
        this.carregarPedidos();
      }
    });
  }

  onFileSelected(event: any) {
    this.ficheiroSelecionado = event.target.files[0];
  }

  confirmarUpload() {
    if (!this.pedidoEmAnalise || !this.ficheiroSelecionado) return;

    this.declaracaoService.resolver(this.pedidoEmAnalise.id, this.ficheiroSelecionado).subscribe({
      next: () => {
        alert('Declaração emitida e enviada!');
        this.isModalUploadAberto = false;
        this.carregarPedidos();
      },
      error: (err) => alert('Erro no upload: ' + err.error?.message)
    });
  }

  getDocumentoUrl(caminho: string): string {
      return `${this.API_BASE_URL}/${caminho}`;
  }
}
