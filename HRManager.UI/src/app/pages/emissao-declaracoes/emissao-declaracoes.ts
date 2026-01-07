import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PedidoDeclaracaoDto } from '../../interfaces/pedidoDeclaracao';
import { DeclaracaoService } from '../../services/declaracao.service';
import { environment } from '../../../environments/environment.prod';

@Component({
  selector: 'app-emissao-declaracoes',
  imports: [FormsModule, CommonModule],
  templateUrl: './emissao-declaracoes.html',
  styleUrl: './emissao-declaracoes.css',
})
export class EmissaoDeclaracoes implements OnInit {
  private declaracaoService = inject(DeclaracaoService);

  public listaPedidos: PedidoDeclaracaoDto[] = [];
  public loading = false;

  ngOnInit() {
    this.carregarPedidos();
  }

  carregarPedidos() {
    this.loading = true;
    this.declaracaoService.getPendentes().subscribe({
      next: (data: PedidoDeclaracaoDto[]) => {
        this.listaPedidos = data;
        this.loading = false;
      },
      error: (err: any) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  aprovarEGerar(pedido: PedidoDeclaracaoDto) {
    if (!confirm(`Gerar declaração automática para ${pedido.nomeColaborador}?`)) return;

    this.loading = true;
    this.declaracaoService.gerarDeclaracao(pedido.id).subscribe({
      next: (blob: Blob) => {
        // Criar link para download
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Declaracao_${pedido.nomeColaborador}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);

        this.loading = false;
        this.carregarPedidos(); // Atualiza a lista
        alert('Declaração gerada com sucesso!');
      },
      error: (err: any) => {
        console.error(err);
        this.loading = false;
        alert('Erro ao gerar documento.');
      }
    });
  }

  rejeitar(pedido: PedidoDeclaracaoDto) {
    if (!confirm('Rejeitar este pedido?')) return;

    this.declaracaoService.atualizarEstado(pedido.id, false).subscribe({
      next: () => {
        this.carregarPedidos();
        alert('Pedido rejeitado.');
      },
      error: (err: any) => alert('Erro ao rejeitar.')
    });
  }

  getDocumentoUrl(caminho: string): string {
    return `${environment.apiUrl}/${caminho}`;
  }
}
