import { Component, OnInit } from '@angular/core';
import { CriarInstituicaoRequest, Instituicao, InstituicaoService } from '../../services/instituicao.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-gestao-instituicoes',
  imports: [CommonModule, FormsModule],
  templateUrl: './gestao-instituicoes.html',
  styleUrl: './gestao-instituicoes.css',
})
export class GestaoInstituicoes implements OnInit {
// Propriedade para guardar a lista de instituições vinda da API
  public listaInstituicoes: Instituicao[] = [];

  // Propriedade para fazer o "data binding" com o formulário
  // Corresponde aos campos do nosso DTO
  public dadosFormulario: CriarInstituicaoRequest = {
    nome: '',
    identificadorUnico: ''
  };

  // --- LÓGICA DE EDIÇÃO ---
  // Guarda o ID da instituição que estamos a editar
  public idInstituicaoEmEdicao: string | null = null;
  // --- FIM LÓGICA DE EDIÇÃO ---

  // Mensagem de feedback (sucesso ou erro)
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  // *** ADICIONAR ESTA PROPRIEDADE ***
  public isModalAberto: boolean = false;

  // Injetar o nosso serviço para o podermos usar
  constructor(private instituicaoService: InstituicaoService) {}

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

  /**
   * Método chamado quando o formulário é submetido.
   * Agora decide se deve "Criar" ou "Atualizar".
   */
  onSubmit(): void {
    this.limparFeedback();

    if (this.idInstituicaoEmEdicao) {
      // --- FLUXO DE ATUALIZAÇÃO ---
      this.instituicaoService.atualizarInstituicao(this.idInstituicaoEmEdicao, this.dadosFormulario).subscribe({
        next: (instituicaoAtualizada) => {
          this.mostrarFeedback(`Instituição "${instituicaoAtualizada.nome}" atualizada com sucesso!`, false);
          this.carregarInstituicoes();
          // *** ADICIONAR LINHA ***
          this.fecharModal();
        },
        error: (err) => {
          this.mostrarFeedback(err.error?.message || 'Erro ao atualizar instituição.', true);
        }
      });
    } else {
      // --- FLUXO DE CRIAÇÃO ---
      this.instituicaoService.criarInstituicao(this.dadosFormulario).subscribe({
        next: (nova) => {
          this.mostrarFeedback(`Instituição "${nova.nome}" criada com sucesso!`, false);
          this.carregarInstituicoes();
          // *** ADICIONAR LINHA ***
          this.fecharModal();
        },
        error: (err) => {
          this.mostrarFeedback(err.error?.message || 'Erro ao criar instituição.', true);
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
      identificadorUnico: instituicao.identificadorUnico
    };
    // *** ADICIONAR LINHA ***
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
    this.dadosFormulario = { nome: '', identificadorUnico: '' };
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
