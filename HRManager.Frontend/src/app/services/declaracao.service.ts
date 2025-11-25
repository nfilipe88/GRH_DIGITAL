import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CriarPedidoDeclaracaoRequest } from '../interfaces/criarPedidoDeclarcaoRequest';
import { PedidoDeclaracaoDto } from '../interfaces/pedidoDeclaracao';

@Injectable({
  providedIn: 'root',
})
export class DeclaracaoService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7234/api/Declaracoes'; // Confirme a porta

  /**
   * Lista pedidos (filtrados pelo backend com base no cargo)
   */
  public getPedidos(): Observable<PedidoDeclaracaoDto[]> {
    return this.http.get<PedidoDeclaracaoDto[]>(this.apiUrl);
  }

  /**
   * Colaborador: Solicita uma nova declaração
   */
  public solicitar(pedido: CriarPedidoDeclaracaoRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, pedido);
  }

  /**
   * Gestor: Resolve o pedido (Upload do PDF ou Rejeição)
   * @param id ID do pedido
   * @param documento Ficheiro PDF assinado (obrigatório para aprovar)
   * @param rejeitar Se true, rejeita o pedido (documento é ignorado)
   */
  public resolver(id: number, documento: File | null, rejeitar: boolean = false): Observable<any> {
    const formData = new FormData();

    if (documento) {
      formData.append('documento', documento);
    }

    // Adicionamos a query string para indicar se é rejeição
    const url = `${this.apiUrl}/${id}/resolver?rejeitar=${rejeitar}`;

    return this.http.put(url, formData);
  }
}
