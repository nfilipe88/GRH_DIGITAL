import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Avaliacao } from '../interfaces/Avaliacao';
import { SubmeterAvaliacaoRequest } from '../interfaces/SubmeterAvaliacaoRequest';

@Injectable({
  providedIn: 'root'
})
export class AvaliacaoService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7234/api/Avaliacoes'; // Ajusta a porta se necessário

  // NOVO: Buscar lista de avaliações da equipa (para o Gestor)
  getAvaliacoesEquipa(): Observable<Avaliacao[]> {
    return this.http.get<Avaliacao[]>(`${this.apiUrl}/equipa`);
  }

  // Buscar uma avaliação específica para preencher (pelo ID da avaliação)
  // Nota: Terás de criar um endpoint GET /api/Avaliacoes/{id} no Backend se ainda não existir,
  // ou usar o método que retorna a lista e filtrar.
  getAvaliacao(id: number): Observable<Avaliacao> {
    return this.http.get<Avaliacao>(`${this.apiUrl}/${id}`);
  }

  // Enviar as notas
  submeterAvaliacao(id: number, request: SubmeterAvaliacaoRequest): Observable<Avaliacao> {
    return this.http.put<Avaliacao>(`${this.apiUrl}/${id}/submeter`, request);
  }
}
