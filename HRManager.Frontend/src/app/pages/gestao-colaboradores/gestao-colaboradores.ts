import { Component, inject, OnInit } from '@angular/core';
import { ColaboradorService } from '../../services/colaborador.service';
import { Instituicao, InstituicaoService } from '../../services/instituicao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';
import { Colaborador } from '../../interfaces/colaborador';
import { CriarColaboradorRequest } from '../../interfaces/criarColaboradorRequest';

@Component({
  selector: 'app-gestao-colaboradores',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './gestao-colaboradores.html',
  styleUrl: './gestao-colaboradores.css',
})
export class GestaoColaboradores implements OnInit {
  private colaboradorService = inject(ColaboradorService);
  private instituicaoService = inject(InstituicaoService);
  private authService = inject(AuthService);

  public dadosFormulario: CriarColaboradorRequest;
  public listaInstituicoes: Instituicao[] = [];
  public listaColaboradores: Colaborador[] = [];

  public isModalAberto: boolean = false;
  public idColaboradorEmEdicao: number | null = null;

  public loggedInUserRole: string | null = null;

  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  public nomeInstituicaoGestor: string = '';

  // Adicione uma variável para controlar se o formulário está pronto
  public isFormReady: boolean = false;

  constructor() {
    this.dadosFormulario = this.criarFormularioVazio();
    this.loggedInUserRole = this.authService.getUserRole();

    // Pré-carregar a instituição do GestorRH se aplicável
    if (this.loggedInUserRole === 'GestorRH') {
      const instituicaoId = this.authService.getInstituicaoId();
      const instituicaoNome = this.authService.getInstituicaoNome();

      if (instituicaoId && instituicaoNome) {
        this.dadosFormulario.instituicaoId = instituicaoId;
        this.nomeInstituicaoGestor = instituicaoNome;
        this.isFormReady = true;
      }
    } else if (this.loggedInUserRole === 'GestorMaster') {
      // Para GestorMaster, o formulário estará pronto após carregar as instituições
      this.isFormReady = true;
    }
  }

  ngOnInit(): void {
    this.carregarColaboradores();

    if (this.loggedInUserRole === 'GestorMaster') {
      this.carregarInstituicoes();
    } else if (this.loggedInUserRole === 'GestorRH') {
      // Já foi pré-carregado no construtor
      console.log('Instituição do GestorRH:', this.nomeInstituicaoGestor, 'ID:', this.dadosFormulario.instituicaoId);
    }
  }

  carregarInstituicoes(): void {
    this.instituicaoService.getInstituicoes().subscribe({
      next: (data) => {
        this.listaInstituicoes = data.filter(inst => inst.isAtiva);
        this.isFormReady = true;
      },
      error: (err) => {
        this.mostrarFeedback('Erro ao carregar instituições.', true);
        this.isFormReady = false;
      }
    });
  }

  carregarColaboradores(): void {
    this.colaboradorService.getColaboradores().subscribe({
      next: (data) => this.listaColaboradores = data,
      error: (err) => console.error('Erro ao carregar colaboradores:', err)
    });
  }

  onSubmit(): void {
    this.limparFeedback();

    // Validação adicional
    if (!this.validarFormulario()) {
      return;
    }

    // Preparar dados para envio
    const dadosParaEnviar = this.prepararDadosParaEnviar();

    if (this.idColaboradorEmEdicao) {
      this.colaboradorService.atualizarColaborador(this.idColaboradorEmEdicao, dadosParaEnviar).subscribe({
        next: (res) => {
          this.mostrarFeedback(res.message || 'Atualizado com sucesso!', false);
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => this.tratarErro(err)
      });
    } else {
      this.colaboradorService.criarColaborador(dadosParaEnviar).subscribe({
        next: (res) => {
          this.mostrarFeedback(res.message || 'Criado com sucesso!', false);
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => this.tratarErro(err)
      });
    }
  }

  abrirModalNovo(): void {
    this.idColaboradorEmEdicao = null;
    this.dadosFormulario = this.criarFormularioVazio();

    // Para GestorRH, preencher automaticamente a instituição
    if (this.loggedInUserRole === 'GestorRH') {
      const instituicaoId = this.authService.getInstituicaoId();
      const instituicaoNome = this.authService.getInstituicaoNome();

      if (instituicaoId && instituicaoNome) {
        this.dadosFormulario.instituicaoId = instituicaoId;
        this.nomeInstituicaoGestor = instituicaoNome;
        console.log('Instituição vinculada no modal:', instituicaoNome, 'ID:', instituicaoId);
      } else {
        this.mostrarFeedback('Erro: Não foi possível identificar a sua instituição. Faça login novamente.', true);
        // Não abrir o modal se não houver instituição
        return;
      }
    }

    this.isModalAberto = true;
  }

  selecionarParaEditar(colaborador: Colaborador): void {
    this.limparFeedback();
    this.idColaboradorEmEdicao = colaborador.id;

    this.colaboradorService.getColaboradorById(colaborador.id).subscribe({
      next: (data) => {
        this.dadosFormulario = {
          nomeCompleto: data.nomeCompleto,
          nif: data.nif,
          numeroAgente: data.numeroAgente,
          emailPessoal: data.emailPessoal,
          telemovel: data.telemovel ? Number(data.telemovel) : null,
          morada: data.morada || '',
          iban: data.iban || '',
          dataNascimento: data.dataNascimento ? data.dataNascimento.split('T')[0] : null,
          dataAdmissao: data.dataAdmissao.split('T')[0],
          cargo: data.cargo,
          tipoContrato: data.tipoContrato,
          salarioBase: data.salarioBase,
          departamento: data.departamento,
          localizacao: data.localizacao,
          instituicaoId: data.instituicaoId
        };

        // Para GestorRH, também mostrar o nome da instituição
        if (this.loggedInUserRole === 'GestorRH') {
          this.nomeInstituicaoGestor = this.authService.getInstituicaoNome() || '';
        }

        this.isModalAberto = true;
      },
      error: (err) => this.mostrarFeedback('Erro ao carregar dados.', true)
    });
  }

  selecionarParaDeletar(colaborador: Colaborador): void {
    if (!confirm(`Eliminar "${colaborador.nomeCompleto}"?`)) return;

    this.colaboradorService.deletarColaborador(colaborador.id).subscribe({
      next: () => {
        this.mostrarFeedback('Colaborador eliminado.', false);
        this.carregarColaboradores();
      },
      error: (err) => this.tratarErro(err)
    });
  }

  mudarEstado(colaborador: Colaborador): void {
    const acao = colaborador.isAtivo ? "desativar" : "reativar";
    if (!confirm(`${acao} "${colaborador.nomeCompleto}"?`)) return;

    this.colaboradorService.atualizarEstadoColaborador(colaborador.id, !colaborador.isAtivo).subscribe({
      next: () => {
        this.mostrarFeedback(`Sucesso ao ${acao}.`, false);
        this.carregarColaboradores();
      },
      error: (err) => this.tratarErro(err)
    });
  }

  fecharModal(): void {
    this.isModalAberto = false;
    this.idColaboradorEmEdicao = null;
    this.limparFeedback();
  }

  // --- Novos métodos auxiliares ---

  private validarFormulario(): boolean {
    // Validação básica
    if (!this.dadosFormulario.nomeCompleto || !this.dadosFormulario.emailPessoal || !this.dadosFormulario.nif) {
      this.mostrarFeedback('Por favor, preencha os campos obrigatórios: Nome, Email e NIF.', true);
      return false;
    }

    // Validação da instituição
    if (!this.dadosFormulario.instituicaoId) {
      this.mostrarFeedback('É necessário selecionar uma instituição.', true);
      return false;
    }

    // Validação do NIF (9 dígitos)
    if (!/^\d{9}$/.test(this.dadosFormulario.nif)) {
      this.mostrarFeedback('O NIF deve conter exatamente 9 dígitos.', true);
      return false;
    }

    return true;
  }

  private prepararDadosParaEnviar(): any {
    const dados = { ...this.dadosFormulario };

    // Converter tipos e limpar dados
    if (dados.telemovel === null || dados.telemovel === undefined) {
      dados.telemovel = null;
    } else {
      dados.telemovel = Number(dados.telemovel);
    }

    if (dados.salarioBase === null || dados.salarioBase === undefined) {
      dados.salarioBase = null;
    } else {
      dados.salarioBase = Number(dados.salarioBase);
    }

    // Garantir que a instituição está definida para GestorRH
    if (this.loggedInUserRole === 'GestorRH' && !dados.instituicaoId) {
      dados.instituicaoId = this.authService.getInstituicaoId();
    }

    return dados;
  }

  private criarFormularioVazio(): CriarColaboradorRequest {
    const formularioVazio = {
      nomeCompleto: '',
      nif: '',
      numeroAgente: null,
      emailPessoal: '',
      morada: '',
      telemovel: null,
      iban: '',
      dataNascimento: null,
      dataAdmissao: new Date().toISOString().split('T')[0],
      cargo: '',
      tipoContrato: '',
      salarioBase: null,
      departamento: '',
      localizacao: '',
      instituicaoId: '',
    };

    // Para GestorRH, já definir a instituição inicial
    if (this.loggedInUserRole === 'GestorRH') {
      const instituicaoId = this.authService.getInstituicaoId();
      if (instituicaoId) {
        formularioVazio.instituicaoId = instituicaoId;
      }
    }

    return formularioVazio;
  }

  private tratarErro(err: any) {
    const msg = err.error?.message || 'Ocorreu um erro.';
    // Se for erro de validação (400), muitas vezes vem em err.error.errors
    if (err.error?.errors) {
      const errors = Object.values(err.error.errors).flat();
      this.mostrarFeedback(errors.join(', '), true);
    } else {
      this.mostrarFeedback(msg, true);
    }
  }

  private mostrarFeedback(mensagem: string, ehErro: boolean): void {
    this.feedbackMessage = mensagem;
    this.isError = ehErro;
  }

  private limparFeedback(): void {
    this.feedbackMessage = null;
    this.isError = false;
  }
}
