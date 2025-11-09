import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

// Adicionei estas interfaces para melhorar a tipagem
export interface Instituicao {
  id: string; // O backend usa Guid, mas no TS/JSON é uma string
  nome: string;
  identificadorUnico: string;
  isAtiva: boolean;
}

export interface CriarInstituicaoRequest {
  nome: string;
  identificadorUnico: string;
}

@Injectable({
  providedIn: 'root',
})
export class InstituicaoService {
  // O URL da nossa API.
  // Certifique-se de que a porta (ex: 5003) corresponde à que o seu backend está a usar.
  private apiUrl = 'http://localhost:5003/api/Instituicoes';

  constructor(private http: HttpClient) { }

  /**
   * Busca a lista de todas as instituições da API
   * Corresponde ao nosso método GET
   */
  public getInstituicoes(): Observable<Instituicao[]> {
    return this.http.get<Instituicao[]>(this.apiUrl);
  }

  /**
   * Envia uma nova instituição para a API
   * Corresponde ao nosso método POST
   * @param instituicao Os dados do formulário (ex: { nome: 'Nome', slug: 'slug' })
   */
  public criarInstituicao(instituicao: CriarInstituicaoRequest): Observable<Instituicao> {
    return this.http.post<Instituicao>(this.apiUrl, instituicao);
  }

  // ---
  // NOVO MÉTODO: Atualizar Instituição (PUT)
  // ---
  public atualizarInstituicao(id: string, instituicao: CriarInstituicaoRequest): Observable<Instituicao> {
    return this.http.put<Instituicao>(`${this.apiUrl}/${id}`, instituicao);
  }

  // ---
  // NOVO MÉTODO: Atualizar Estado (PATCH)
  // ---
  public atualizarEstado(id: string, isAtiva: boolean): Observable<any> {
    const request = { isAtiva: isAtiva };
    return this.http.patch(`${this.apiUrl}/${id}/estado`, request);
  }
}
