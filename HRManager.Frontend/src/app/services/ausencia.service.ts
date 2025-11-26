import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AusenciaDto } from '../interfaces/ausenciaDto';
import { ResponderAusenciaRequest } from '../interfaces/responderAusenciaRequest';
import { CriarAusenciaRequest } from '../interfaces/criarAusenciaRequest';
import { AusenciaSaldoDto } from '../interfaces/ausenciaSaldoDto';

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
  // public solicitarAusencia(pedido: CriarAusenciaRequest): Observable<any> {
  //   return this.http.post<any>(this.apiUrl, pedido);
  // }

  public solicitarAusencia(pedido: CriarAusenciaRequest): Observable<any> {
    // Para enviar ficheiros, temos de usar FormData
    const formData = new FormData();

    formData.append('Tipo', pedido.tipo);
    formData.append('DataInicio', pedido.dataInicio);
    formData.append('DataFim', pedido.dataFim);

    if (pedido.motivo) {
      formData.append('Motivo', pedido.motivo);
    }

    if (pedido.documento) {
      // 'Documento' deve coincidir com o nome da propriedade no DTO do C#
      formData.append('Documento', pedido.documento);
    }

    return this.http.post<any>(this.apiUrl, formData);
  }

  /**
   * Aprova ou Rejeita um pedido
   */
  public responderAusencia(id: number, resposta: ResponderAusenciaRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/responder`, resposta);
  }

  // *** 2. MÉTODO Vaidar dias de saldo para ferias***
  public getSaldo(): Observable<AusenciaSaldoDto> {
    return this.http.get<AusenciaSaldoDto>(`${this.apiUrl}/saldo`);
  }

  /**
   * Faz o download do relatório Excel
   */
  public downloadRelatorioExcel(mes: number, ano: number): Observable<Blob> {
    // Importante: Definir responseType como 'blob' para o Angular saber que é um ficheiro
    const url = `https://localhost:7234/api/Relatorios/ausencias?mes=${mes}&ano=${ano}`;
    return this.http.get(url, { responseType: 'blob' });
  }
}
