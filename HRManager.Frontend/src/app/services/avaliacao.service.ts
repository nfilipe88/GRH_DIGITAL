import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Avaliacao } from '../interfaces/Avaliacao';
import { environment } from '../../environments/environment';
import { RealizarAutoAvaliacaoRequest } from '../interfaces/realizarAutoAvaliacaoRequest';
import { RealizarAvaliacaoGestorRequest } from '../interfaces/realizarAvaliacaoGestorRequest';
import { Competencia, CriarCompetenciaRequest, CicloAvaliacao, CriarCicloRequest } from '../interfaces/configuracao-avaliacao';

@Injectable({
  providedIn: 'root'
})
export class AvaliacaoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Avaliacoes`;
  // --- LEITURA ---
  getAvaliacoesEquipa(): Observable<Avaliacao[]> {
    return this.http.get<Avaliacao[]>(`${this.apiUrl}/equipa`);
  }

  getMinhasAvaliacoes(): Observable<Avaliacao[]> {
    return this.http.get<Avaliacao[]>(`${this.apiUrl}/minhas`);
  }

  getAvaliacao(id: string): Observable<Avaliacao> {
    // Atenção: O backend precisa de ter este endpoint GET /Avaliacoes/{id}
    // Se não tiveres criado, terás de usar getMinhasAvaliacoes e filtrar no JS,
    // ou criar o endpoint GetById no AvaliacoesController.
    return this.http.get<Avaliacao>(`${this.apiUrl}/${id}`);
  }

  // --- AÇÕES ---
  iniciarAvaliacao(colaboradorId: string, cicloId: string): Observable<Avaliacao> {
      return this.http.post<Avaliacao>(`${this.apiUrl}/iniciar?colaboradorId=${colaboradorId}&cicloId=${cicloId}`, {});
  }

  // Ação do Colaborador
  submeterAutoAvaliacao(id: string, request: RealizarAutoAvaliacaoRequest): Observable<Avaliacao> {
    return this.http.put<Avaliacao>(`${this.apiUrl}/${id}/auto-avaliacao`, request);
  }

  // Ação do Gestor
  submeterAvaliacaoGestor(id: string, request: RealizarAvaliacaoGestorRequest): Observable<Avaliacao> {
    return this.http.put<Avaliacao>(`${this.apiUrl}/${id}/avaliacao-gestor`, request);
  }

  // === GESTÃO DE COMPETÊNCIAS ===
  getCompetencias(): Observable<Competencia[]> {
    return this.http.get<Competencia[]>(`${this.apiUrl}/competencias`);
  }

  criarCompetencia(request: CriarCompetenciaRequest): Observable<Competencia> {
    return this.http.post<Competencia>(`${this.apiUrl}/competencias`, request);
  }

  // === GESTÃO DE CICLOS ===
  getCiclos(): Observable<CicloAvaliacao[]> {
    return this.http.get<CicloAvaliacao[]>(`${this.apiUrl}/ciclos`);
  }

  criarCiclo(request: CriarCicloRequest): Observable<CicloAvaliacao> {
    return this.http.post<CicloAvaliacao>(`${this.apiUrl}/ciclos`, request);
  }
}
