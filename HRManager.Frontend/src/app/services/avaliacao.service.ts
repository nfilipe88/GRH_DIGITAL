import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AvaliacaoDto } from '../interfaces/AvaliacaoDto';
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
  getAvaliacoesEquipa(): Observable<AvaliacaoDto[]> {
    return this.http.get<AvaliacaoDto[]>(`${this.apiUrl}/equipa`);
  }

  getMinhasAvaliacoes(): Observable<AvaliacaoDto[]> {
    return this.http.get<AvaliacaoDto[]>(`${this.apiUrl}/minhas`);
  }

  getAvaliacaoById(id: string): Observable<AvaliacaoDto> {
    // Atenção: O backend precisa de ter este endpoint GET /Avaliacoes/{id}
    // Se não tiveres criado, terás de usar getMinhasAvaliacoes e filtrar no JS,
    // ou criar o endpoint GetById no AvaliacoesController.
    return this.http.get<AvaliacaoDto>(`${this.apiUrl}/${id}`);
  }

  // --- AÇÕES ---
  iniciarAvaliacao(colaboradorId: string, cicloId: string): Observable<AvaliacaoDto> {
      return this.http.post<AvaliacaoDto>(`${this.apiUrl}/iniciar?colaboradorId=${colaboradorId}&cicloId=${cicloId}`, {});
  }

  // Ação do Colaborador
  realizarAutoAvaliacao(id: string, request: RealizarAutoAvaliacaoRequest): Observable<AvaliacaoDto> {
    return this.http.put<AvaliacaoDto>(`${this.apiUrl}/${id}/auto-avaliacao`, request);
  }

  // Ação do Gestor
  submeterAvaliacaoGestor(id: string, request: RealizarAvaliacaoGestorRequest): Observable<AvaliacaoDto> {
    return this.http.put<AvaliacaoDto>(`${this.apiUrl}/${id}/avaliacao-gestor`, request);
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
