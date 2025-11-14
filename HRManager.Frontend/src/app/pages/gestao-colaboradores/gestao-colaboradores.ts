import { Component, OnInit } from '@angular/core';
import { Colaborador, ColaboradorService, CriarColaboradorRequest } from '../../services/colaborador.service';
import { Instituicao, InstituicaoService } from '../../services/instituicao.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-gestao-colaboradores',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './gestao-colaboradores.html',
  styleUrl: './gestao-colaboradores.css',
})
export class GestaoColaboradores implements OnInit {

  // --- Propriedades para o Formulário ---
  public dadosFormulario: CriarColaboradorRequest;

  // --- Propriedades para o Dropdown ---
  public listaInstituicoes: Instituicao[] = [];
  public listaColaboradores: Colaborador[] = [];

  // --- Propriedades de Feedback ---
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  // *** ADICIONAR ESTA PROPRIEDADE ***
  public isModalAberto: boolean = false;

  // *** 2. ADICIONAR ESTADO DE EDIÇÃO ***
  public idColaboradorEmEdicao: number | null = null;
  public feedbackModal: string | null = null;
  public isErrorModal: boolean = false;

  constructor(
    private colaboradorService: ColaboradorService,
    private instituicaoService: InstituicaoService // Injetar o serviço
  ) {
    // Inicializar o formulário
    this.dadosFormulario = this.criarFormularioVazio();
  }

  ngOnInit(): void {
    this.carregarInstituicoes();
    this.carregarColaboradores(); // *** CORREÇÃO: ADICIONAR ESTA LINHA ***
  }

  /**
   * Busca as instituições ativas para preencher o dropdown
   */
  carregarInstituicoes(): void {
    this.instituicaoService.getInstituicoes().subscribe({
      next: (data) => {
        // Filtramos para mostrar apenas instituições ATIVAS no dropdown
        this.listaInstituicoes = data.filter(inst => inst.isAtiva);
      },
      error: (err) => {
        this.mostrarFeedback('Erro fatal: Não foi possível carregar as instituições.', true);
      }
    });
  }

  /**
   * *** NOVO MÉTODO: Busca os colaboradores para a tabela ***
   */
  carregarColaboradores(): void {
    this.colaboradorService.getColaboradores().subscribe({
      next: (data) => {
        this.listaColaboradores = data;
      },
      error: (err) => {
        // Não mostramos um erro fatal aqui, apenas na consola
        console.error('Erro ao carregar colaboradores:', err);
        this.mostrarFeedback('Erro ao carregar a lista de colaboradores.', true); // *** MELHORIA: Mostrar feedback de erro ***
      }
    });
  }

  /**
   * Chamado ao submeter o formulário (CRIAR ou ATUALIZAR)
   */
  onSubmit(): void {
    this.limparFeedback(true); // Limpa só o feedback do modal

    if (this.idColaboradorEmEdicao) {
      // --- FLUXO DE ATUALIZAÇÃO ---
      this.colaboradorService.atualizarColaborador(this.idColaboradorEmEdicao, this.dadosFormulario).subscribe({
        next: (response) => {
          this.mostrarFeedback(response.message || 'Colaborador atualizado com sucesso!', false); // Feedback na página
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => {
          const msg = err.error?.message || 'Erro ao atualizar colaborador.';
          this.mostrarFeedback(msg, true, true); // Feedback DENTRO do modal
        }
      });
    } else {
      // --- FLUXO DE CRIAÇÃO ---
      this.colaboradorService.criarColaborador(this.dadosFormulario).subscribe({
        next: (response) => {
          this.mostrarFeedback(response.message || 'Colaborador criado com sucesso!', false); // Feedback na página
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => {
          const msg = err.error?.message || 'Erro ao criar colaborador.';
          this.mostrarFeedback(msg, true, true); // Feedback DENTRO do modal
        }
      });
    }
  }

  /**
   * Abre o modal para EDITAR um colaborador existente
   */
  public selecionarParaEditar(colaborador: Colaborador): void {
    this.limparFeedback(true);
    this.idColaboradorEmEdicao = colaborador.id; // Define o modo de edição

    // 1. Vai buscar os dados completos à API
    this.colaboradorService.getColaboradorById(colaborador.id).subscribe({
      next: (data) => {
        // 2. Preenche o formulário com os dados
        this.dadosFormulario = {
          nomeCompleto: data.nomeCompleto,
          nif: data.nif,
          numeroAgente: data.numeroAgente,
          emailPessoal: data.emailPessoal,
          // Precisamos formatar a data para o <input type="date"> (yyyy-MM-dd)
          dataNascimento: data.dataNascimento ? data.dataNascimento.split('T')[0] : null,
          dataAdmissao: data.dataAdmissao.split('T')[0],
          cargo: data.cargo,
          tipoContrato: data.tipoContrato,
          salarioBase: data.salarioBase,
          departamento: data.departamento,
          localizacao: data.localizacao,
          instituicaoId: data.instituicaoId
        };
        // 3. Abre o modal
        this.isModalAberto = true;
      },
      error: (err) => {
        this.mostrarFeedback('Não foi possível carregar os dados para edição.', true);
      }
    });
  }

  /**
   * *** MÉTODO SUBSTITUÍDO ***
   * Pede confirmação e Desativa/Reativa um colaborador
   */
  public mudarEstado(colaborador: Colaborador): void {
    const acao = colaborador.isAtivo ? "desativar" : "reativar";

    if (!confirm(`Tem a certeza que deseja ${acao} "${colaborador.nomeCompleto}"?`)) {
      return;
    }

    const novoEstado = !colaborador.isAtivo;

    this.colaboradorService.atualizarEstadoColaborador(colaborador.id, novoEstado).subscribe({
      next: (response) => {
        this.mostrarFeedback(response.message || `Colaborador ${acao}do com sucesso.`, false);
        this.carregarColaboradores(); // Recarrega a lista
      },
      error: (err) => {
        const msg = err.error?.message || `Erro ao ${acao} colaborador.`;
        this.mostrarFeedback(msg, true);
      }
    });
  }

  /**
   * Pede confirmação e elimina um colaborador
   */
  public selecionarParaDeletar(colaborador: Colaborador): void {
    if (!confirm(`Tem a certeza que deseja eliminar "${colaborador.nomeCompleto}"? Esta ação é irreversível.`)) {
      return;
    }
    this.colaboradorService.deletarColaborador(colaborador.id).subscribe({
      next: (response) => {
        this.mostrarFeedback(response.message || 'Colaborador eliminado com sucesso.', false);
        this.carregarColaboradores(); // Recarrega a lista
      },
      error: (err) => {
        const msg = err.error?.message || 'Erro ao eliminar colaborador.';
        this.mostrarFeedback(msg, true);
      }
    });
  }

  /**
   * Abre o modal para criar um NOVO colaborador
   */
  public abrirModalNovo(): void {
    this.idColaboradorEmEdicao = null; // Garante que não está em modo de edição
    this.dadosFormulario = this.criarFormularioVazio();
    this.limparFeedback(true);
    this.isModalAberto = true;
  }

  /**
   * Fecha o modal
   */
  public fecharModal(): void {
    this.isModalAberto = false;
    this.idColaboradorEmEdicao = null; // Sai do modo de edição
    this.limparFeedback(true);
  }

  // --- Métodos Auxiliares ---

  private criarFormularioVazio(): CriarColaboradorRequest {
    return {
      nomeCompleto: '',
      nif: '',
      numeroAgente: null,
      emailPessoal: '',
      dataNascimento: null,
      dataAdmissao: new Date().toISOString().split('T')[0], // Pré-preenche com a data de hoje
      cargo: '',
      tipoContrato: '',
      salarioBase: null,
      departamento: '',
      localizacao: '',
      instituicaoId: '' // Começa vazio
    };
  }

  private mostrarFeedback(mensagem: string, ehErro: boolean, noModal: boolean = false): void {
    if (noModal) {
      this.feedbackModal = mensagem;
      this.isErrorModal = ehErro;
    } else {
      this.feedbackMessage = mensagem;
      this.isError = ehErro;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  private limparFeedback(apenasModal: boolean = false): void {
    if (apenasModal) {
      this.feedbackModal = null;
      this.isErrorModal = false;
    } else {
      this.feedbackMessage = null;
      this.isError = false;
      this.feedbackModal = null;
      this.isErrorModal = false;
    }
  }
}
