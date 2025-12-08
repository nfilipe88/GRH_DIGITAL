import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CriarPedidoDeclaracaoRequest } from '../interfaces/criarPedidoDeclarcaoRequest';
import { PedidoDeclaracaoDto } from '../interfaces/pedidoDeclaracao';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class DeclaracaoService {
  private http = inject(HttpClient);
  // private apiUrl = 'https://localhost:7234/api/Declaracoes'; // Confirme a porta
  private apiUrl = `${environment.apiUrl}/Declaracoes`;

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
  public resolver(id: number, ficheiro: File | null, rejeitar: boolean= false): Observable<any> {

    // Se for rejeição, basta mandar o flag na Query String
    if (rejeitar) {
      return this.http.put(`${this.apiUrl}/${id}/resolver?rejeitar=true`, {});
    }

    // Se for aprovação, temos de enviar o ficheiro via FormData
    const formData = new FormData();
    if (ficheiro) {
      formData.append('documento', ficheiro); // 'documento' deve bater certo com [FromForm] IFormFile documento no Controller
    }

    return this.http.put(`${this.apiUrl}/${id}/resolver?rejeitar=false`, formData);
  }
}
