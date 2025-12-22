import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Colaborador } from '../interfaces/colaborador';
import { ColaboradorDetails } from '../interfaces/colaboradorDetails';
import { CriarColaboradorRequest } from '../interfaces/criarColaboradorRequest';
import { environment } from '../../environments/environment';
import { UserDetailsDto } from '../interfaces/userDetailsDto';

@Injectable({
  providedIn: 'root'
})
export class ColaboradorService {
  private apiUrl = `${environment.apiUrl}/Colaboradores`;

  constructor(private http: HttpClient) { }

  /**
   * Busca a lista de todos os colaboradores
   */
  public getColaboradores(): Observable<Colaborador[]> {
    return this.http.get<Colaborador[]>(this.apiUrl);
  }

  /**
   * Envia um novo colaborador para a API
   * Corresponde ao nosso método POST
   */
  public criarColaborador(colaborador: CriarColaboradorRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, colaborador);
  }

  // ---
  // *** MÉTODO: Buscar um colaborador por ID ***
  // ---
  public getColaboradorById(id: string): Observable<ColaboradorDetails> {
    return this.http.get<ColaboradorDetails>(`${this.apiUrl}/${id}`);
  }

  // ---
  // *** MÉTODO: Atualizar um colaborador ***
  // ---
  public atualizarColaborador(id: string, colaborador: CriarColaboradorRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, colaborador);
  }

  // ---
  // *** MÉTODO SUBSTITUÍDO ***
  // ---
  public atualizarEstadoColaborador(id: string, isAtiva: boolean): Observable<any> {
    const request = { isAtiva: isAtiva };
    return this.http.patch<any>(`${this.apiUrl}/${id}/estado`, request);
  }

  // ---
  // *** MÉTODO: Eliminar um colaborador ***
  // ---
  public deletarColaborador(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  // ADICIONAR: Método para desativar
  desativar(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/desativar`, {});
  }

  // ADICIONAR: Método para transferir equipa
  transferirEquipa(gestorAntigoId: string, gestorNovoId: string): Observable<any> {
    // Passamos via Query Params conforme definimos no Controller C#
    return this.http.put(`${this.apiUrl}/transferir-equipa?gestorAntigoId=${gestorAntigoId}&gestorNovoId=${gestorNovoId}`, {});
  }
}
