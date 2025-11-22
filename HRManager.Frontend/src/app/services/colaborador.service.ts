import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Colaborador } from '../interfaces/colaborador';
import { ColaboradorDetails } from '../interfaces/colaboradorDetails';
import { CriarColaboradorRequest } from '../interfaces/criarColaboradorRequest';

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
