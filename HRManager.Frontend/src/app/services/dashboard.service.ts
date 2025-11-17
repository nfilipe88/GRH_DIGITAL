import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardStats } from '../interfaces/dashboard-stats';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7234/api/Dashboard'; // Use a sua porta

  // 2. Método para buscar os dados
  public getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
  }
}
