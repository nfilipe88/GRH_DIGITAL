import { Component, OnInit } from '@angular/core';
import { InstituicaoService } from '../../services/instituicao.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CriarInstituicaoRequest } from '../../interfaces/criarInstituicaoRequest';
import { Instituicao } from '../../interfaces/instituicao';


@Component({
  selector: 'app-gestao-instituicoes',
  imports: [CommonModule, FormsModule],
  templateUrl: './gestao-instituicoes.html',
  styleUrl: './gestao-instituicoes.css',
})
export class GestaoInstituicoes implements OnInit {
  public listaInstituicoes: Instituicao[] = [];
  // Propriedade para fazer o "data binding" com o formulário
  // Corresponde aos campos do nosso DTO
  public dadosFormulario = {
    // id: null,
    nome: '',
    identificadorUnico: '',
    nif: '',
    endereco: '',
    telemovel: null as number | null,
    emailContato: ''
  };

  // --- LÓGICA DE EDIÇÃO ---
  // Guarda o ID da instituição que estamos a editar
  public idInstituicaoEmEdicao: string | null = null;

  // Mensagem de feedback (sucesso ou erro)
  public feedbackMessage: string | null = null;
  public isError: boolean = false;
  public isModalAberto: boolean = false;

  // Injetar o nosso serviço para o podermos usar
  constructor(private instituicaoService: InstituicaoService) { }

  // ngOnInit é chamado automaticamente quando o componente é carregado
  ngOnInit(): void {
    this.carregarInstituicoes();
  }

  /**
   * Método para carregar a lista de instituições do serviço.
   */
  carregarInstituicoes(): void {
    this.instituicaoService.getInstituicoes().subscribe({
      next: (data) => {
        this.listaInstituicoes = data;
      },
      error: (err) => {
        console.error('Erro ao carregar instituições:', err);
        this.feedbackMessage = 'Falha ao carregar a lista de instituições.';
        this.isError = true;
      }
    });
  }

  // --- MÉTODO: Gerar Slug automaticamente ---
  // Ligue isto ao (input) do campo Nome no HTML ou chame no onSubmit
  public gerarIdentificador(): void {
    if (this.dadosFormulario.nome) {
      this.dadosFormulario.identificadorUnico = this.dadosFormulario.nome
        .toUpperCase()
        .trim()
        .replace(/[^\w\s-]/g, '') // Remove caracteres especiais
        .replace(/[\s_-]+/g, '-') // Substitui espaços por hifens
        .replace(/^-+|-+$/g, ''); // Remove hifens nas pontas
    }
  }

  /**
   * Método chamado quando o formulário é submetido.
   * Agora decide se deve "Criar" ou "Atualizar".
   */
  onSubmit(): void {
    this.limparFeedback();
    // 1. Garantir que o Slug existe (se o utilizador não preencheu, geramos agora)
    if (!this.dadosFormulario.identificadorUnico) {
      this.gerarIdentificador();
    }

    // 2. Limpeza de dados (Sanitization)
    const requestData = { ...this.dadosFormulario };

    if (this.idInstituicaoEmEdicao) {
      // --- FLUXO DE ATUALIZAÇÃO ---
      this.instituicaoService.atualizarInstituicao(this.idInstituicaoEmEdicao, requestData).subscribe({
        next: (instituicaoAtualizada) => {
          this.mostrarFeedback(`Instituição "${instituicaoAtualizada.nome}" atualizada com sucesso!`, false);
          this.carregarInstituicoes();
          this.fecharModal();
        },
        error: (err) => {
          this.mostrarFeedback(err.error?.message || 'Erro ao atualizar instituição.', true);
        }
      });
    } else {
      // --- FLUXO DE CRIAÇÃO ---
      this.instituicaoService.criarInstituicao(requestData).subscribe({
        next: (nova) => {
          this.mostrarFeedback(`Instituição "${nova.nome}" criada com sucesso!`, false);
          this.carregarInstituicoes();
          this.fecharModal();
        },
        error: (err) => {
          this.mostrarFeedback(err.error?.message || 'Erro ao criar instituição.', true);
          console.error('Erro Backend:', err.error);
          const msg = err.error?.errors ? JSON.stringify(err.error.errors) : 'Erro ao criar';
          alert(msg);
        }
      });
    }
  }

  /**
   * Prepara o formulário para edição.
   */
  public selecionarParaEditar(instituicao: Instituicao): void {
    this.idInstituicaoEmEdicao = instituicao.id;
    this.dadosFormulario = {
      nome: instituicao.nome,
      identificadorUnico: instituicao.identificadorUnico,
      nif: instituicao.nif,
      endereco: instituicao.endereco,
      telemovel: instituicao.telemovel ? Number(instituicao.telemovel) : null,
      emailContato: instituicao.emailContato
    };
    this.isModalAberto = true;
    this.limparFeedback();
  }

  /**
   * NOVO MÉTODO: Chamado para Ativar ou Inativar.
   */
  public mudarEstado(instituicao: Instituicao): void {
    // Pergunta de confirmação
    const acao = instituicao.isAtiva ? "inativa" : "reativa";
    if (!confirm(`Tem a certeza que deseja ${acao} a instituição "${instituicao.nome}"?`)) {
      return;
    }

    const novoEstado = !instituicao.isAtiva;

    this.instituicaoService.atualizarEstado(instituicao.id, novoEstado).subscribe({
      next: () => {
        this.mostrarFeedback(`Instituição ${acao}da com sucesso.`, false);
        // Atualiza a lista localmente para resposta imediata (ou pode recarregar)
        const index = this.listaInstituicoes.findIndex(i => i.id === instituicao.id);
        if (index > -1) {
          this.listaInstituicoes[index].isAtiva = novoEstado;
        }
      },
      error: (err) => {
        this.mostrarFeedback(err.error?.message || `Erro ao ${acao} instituição.`, true);
      }
    });
  }

  /**
   * Abre o modal para criar uma NOVA instituição
   */
  public abrirModalNovo(): void {
    this.resetarFormulario();
    this.limparFeedback();
    this.isModalAberto = true;
  }

  /**
   * Fecha o modal e limpa tudo
   */
  public fecharModal(): void {
    this.isModalAberto = false;
    this.resetarFormulario();
    this.limparFeedback();
  }

  // --- Métodos Auxiliares ---

  private resetarFormulario(): void {
    this.idInstituicaoEmEdicao = null;
    this.dadosFormulario = {
      nome: '',
      identificadorUnico: '',
      nif: '',
      endereco: '',
      telemovel: null as number | null,
      emailContato: ''
    };
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
