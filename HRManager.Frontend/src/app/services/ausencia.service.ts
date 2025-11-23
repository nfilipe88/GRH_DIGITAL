import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AusenciaDto } from '../interfaces/ausenciaDto';
import { ResponderAusenciaRequest } from '../interfaces/responderAusenciaRequest';
import { CriarAusenciaRequest } from '../interfaces/criarAusenciaRequest';

@Injectable({
  providedIn: 'root',
})
export class AusenciaService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7234/api/Ausencias'; // Confirme a sua porta!

  /**
   * Busca a lista de ausências (O backend decide se mostra todas ou só as minhas)
   */
  public getAusencias(): Observable<AusenciaDto[]> {
    return this.http.get<AusenciaDto[]>(this.apiUrl);
  }

  /**
   * Envia um novo pedido
   */
  public solicitarAusencia(pedido: CriarAusenciaRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, pedido);
  }

  /**
   * Aprova ou Rejeita um pedido
   */
  public responderAusencia(id: number, resposta: ResponderAusenciaRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/responder`, resposta);
  }
}
