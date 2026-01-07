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
  private apiUrl = `${environment.apiUrl}/Declaracoes`;

  // --- COLABORADOR ---

  solicitar(request: CriarPedidoDeclaracaoRequest): Observable<PedidoDeclaracaoDto> {
    return this.http.post<PedidoDeclaracaoDto>(`${this.apiUrl}/solicitar`, request);
  }

  // Método correto para o colaborador ver as suas
  getMinhas(): Observable<PedidoDeclaracaoDto[]> {
    return this.http.get<PedidoDeclaracaoDto[]>(`${this.apiUrl}/minhas`);
  }

  // --- GESTOR RH ---

  // Método correto para o gestor ver as pendentes
  getPendentes(): Observable<PedidoDeclaracaoDto[]> {
    return this.http.get<PedidoDeclaracaoDto[]>(`${this.apiUrl}/pendentes`);
  }

  // Aprovar e Gerar PDF
  gerarDeclaracao(id: string): Observable<Blob> {
    return this.http.put(`${this.apiUrl}/${id}/gerar`, {}, { responseType: 'blob' });
  }

  // Rejeitar ou alterar estado sem gerar PDF
  atualizarEstado(id: string, aprovado: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/estado`, aprovado);
  }
}
