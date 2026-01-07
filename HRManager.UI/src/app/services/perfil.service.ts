import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CriarCertificacaoRequest } from '../interfaces/criarCertificacaoRequest';
import { CriarHabilitacaoRequest } from '../interfaces/criarHabilitacaoRequest';
import { PerfilDto } from '../interfaces/perfilDto';
import { AtualizarDadosPessoaisRequest } from '../interfaces/atualizarDadosPessoaisRequest';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class PerfilService {
  private http = inject(HttpClient);
  // private apiUrl = 'https://localhost:7234/api/Perfil'; // Confirme a sua porta!
  private apiUrl = `${environment.apiUrl}/Perfil`;

  /**
   * Obtém o perfil.
   * @param colaboradorId (Opcional) Se for Gestor, pode passar o ID de outro colaborador.
   */
  public getPerfil(colaboradorId?: string): Observable<PerfilDto> {
    let url = this.apiUrl;
    if (colaboradorId) {
      url += `?colaboradorId=${colaboradorId}`;
    }
    return this.http.get<PerfilDto>(url);
  }

  /**
   * Adiciona uma Habilitação Literaria (com upload)
   */
  public addHabilitacao(dados: CriarHabilitacaoRequest, colaboradorId?: string): Observable<any> {
    const formData = new FormData();
    formData.append('Grau', dados.grau);
    formData.append('Curso', dados.curso);
    formData.append('InstituicaoEnsino', dados.instituicaoEnsino);
    formData.append('DataConclusao', dados.dataConclusao);

    if (dados.documento) {
      formData.append('Documento', dados.documento);
    }

    let url = `${this.apiUrl}/habilitacoes`;
    if (colaboradorId) url += `?colaboradorId=${colaboradorId}`;

    return this.http.post(url, formData);
  }

  /**
   * Adiciona uma Certificação Profissional (com upload)
   */
  public addCertificacao(dados: CriarCertificacaoRequest, colaboradorId?: string): Observable<any> {
    const formData = new FormData();
    formData.append('NomeCertificacao', dados.nomeCertificacao);
    formData.append('EntidadeEmissora', dados.entidadeEmissora);
    formData.append('DataEmissao', dados.dataEmissao);

    if (dados.dataValidade) {
      formData.append('DataValidade', dados.dataValidade);
    }
    if (dados.documento) {
      formData.append('Documento', dados.documento);
    }

    let url = `${this.apiUrl}/certificacoes`;
    if (colaboradorId) url += `?colaboradorId=${colaboradorId}`;

    return this.http.post(url, formData);
  }

  /**
   * Remove uma habilitação
   */
  public deleteHabilitacao(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/habilitacoes/${id}`);
  }

  /**
   * Remove uma certificação
   */
  public deleteCertificacao(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/certificacoes/${id}`);
  }

  /**
   * Atualiza Morada e IBAN
   */
  public updateDadosPessoais(dados: AtualizarDadosPessoaisRequest, colaboradorId?: string): Observable<any> {
    let url = `${this.apiUrl}/dados-pessoais`;
    if (colaboradorId) url += `?colaboradorId=${colaboradorId}`;

    return this.http.put(url, dados);
  }
}
