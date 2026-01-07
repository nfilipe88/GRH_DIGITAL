import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Notificacao } from '../interfaces/notificacao';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class NotificacaoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Notificacoes`;

  // Estado reativo para o contador de não lidas
  public totalNaoLidas = new BehaviorSubject<number>(0);

  getMinhas(): Observable<Notificacao[]> {
    return this.http.get<Notificacao[]>(this.apiUrl).pipe(
      tap(lista => this.totalNaoLidas.next(lista.length))
    );
  }

  marcarLida(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/ler`, {}).pipe(
      tap(() => {
        // Decrementa localmente para ser rápido
        const atual = this.totalNaoLidas.value;
        if (atual > 0) this.totalNaoLidas.next(atual - 1);
      })
    );
  }
}
