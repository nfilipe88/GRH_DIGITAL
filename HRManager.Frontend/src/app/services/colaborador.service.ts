import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

// Interface para o nosso DTO
export interface CriarColaboradorRequest {
  nomeCompleto: string;
  nif: string;
  numeroAgente?: number | null;
  emailPessoal: string;
  dataNascimento?: string | null; // Datas são enviadas como string
  dataAdmissao: string; //
  cargo?: string | null;
  tipoContrato?: string | null;
  salarioBase?: number | null;
  departamento?: string | null;
  localizacao?: string | null;
  instituicaoId: string; // Guid é enviado como string
}

// *** INTERFACE ATUALIZADA ***
// Corresponde ao nosso novo ColaboradorListDto
export interface Colaborador {
  id: number;
  nomeCompleto: string;
  emailPessoal: string;
  nif: string;
  nomeInstituicao: string; // <-- Mudança principal
  cargo: string | null;
  isAtivo: boolean;
}

// *** NOVA INTERFACE: O modelo COMPLETO do Colaborador ***
// Isto é o que recebemos do endpoint GET /api/Colaboradores/{id}
export interface ColaboradorDetails {
  id: number;
  nomeCompleto: string;
  nif: string;
  numeroAgente: number | null;
  emailPessoal: string;
  dataNascimento: string | null; // Vem como string ISO
  dataAdmissao: string; // Vem como string ISO
  cargo: string | null;
  tipoContrato: string | null;
  salarioBase: number | null;
  departamento: string | null;
  localizacao: string | null;
  instituicaoId: string;
}

@Injectable({
  providedIn: 'root'
})
export class ColaboradorService {

  // private apiUrl = 'http://localhost:5003/api/Colaboradores'; // Use a porta correta!
  private apiUrl = 'https://localhost:7234/api/Colaboradores'; // Use a porta correta! https

  constructor(private http: HttpClient) { }

  /**
   * *** NOVO MÉTODO ***
   * Busca a lista de todos os colaboradores
   */
  public getColaboradores(): Observable<Colaborador[]> {
    return this.http.get<Colaborador[]>(this.apiUrl);
  }
  // public getColaboradores(page: number = 1, pageSize: number = 50): Observable<{ data: Colaborador[], total: number }> {
  //   return this.http.get<{ data: Colaborador[], total: number }>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  // }

  /**
   * Envia um novo colaborador para a API
   * Corresponde ao nosso método POST
   */
  public criarColaborador(colaborador: CriarColaboradorRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, colaborador);
  }

  // ---
  // *** NOVO MÉTODO: Buscar um colaborador por ID ***
  // ---
  public getColaboradorById(id: number): Observable<ColaboradorDetails> {
    return this.http.get<ColaboradorDetails>(`${this.apiUrl}/${id}`);
  }

  // ---
  // *** NOVO MÉTODO: Atualizar um colaborador ***
  // ---
  public atualizarColaborador(id: number, colaborador: CriarColaboradorRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, colaborador);
  }

  // ---
  // *** MÉTODO SUBSTITUÍDO ***
  // ---
  public atualizarEstadoColaborador(id: number, isAtiva: boolean): Observable<any> {
    const request = { isAtiva: isAtiva };
    // Usamos o novo endpoint de ESTADO
    return this.http.patch<any>(`${this.apiUrl}/${id}/estado`, request);
  }

  // ---
  // *** NOVO MÉTODO: Eliminar um colaborador ***
  // ---
  public deletarColaborador(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
