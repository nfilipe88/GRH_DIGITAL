import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ColaboradorService } from '../../services/colaborador.service';
import { InstituicaoService } from '../../services/instituicao.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';
import { Colaborador } from '../../interfaces/colaborador';
import { Instituicao } from '../../interfaces/instituicao';
import { CargoDto } from '../../interfaces/cargoDto';
import { ColaboradorListDto } from '../../interfaces/colaboradorListDto';

@Component({
  selector: 'app-gestao-colaboradores',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule],
  templateUrl: './gestao-colaboradores.html',
  styleUrl: './gestao-colaboradores.css',
})
export class GestaoColaboradores implements OnInit {
  // Injeção de dependências
  private fb = inject(FormBuilder);
  private colaboradorService = inject(ColaboradorService);
  private instituicaoService = inject(InstituicaoService);
  private authService = inject(AuthService);

  // Variáveis de Estado
  public colaboradorForm: FormGroup; // O nosso novo formulário Reativo
  public listaInstituicoes: Instituicao[] = [];
  public listaColaboradores: Colaborador[] = [];

  public isModalAberto: boolean = false;
  public idColaboradorEmEdicao: string = '';
  public loggedInUserRole: string | null = null;
  public nomeInstituicaoGestor: string = '';

  // Estado
  isMaster = signal<boolean>(false);
  instituicoes = signal<Instituicao[]>([]);
  cargos = signal<CargoDto[]>([]); // Lista completa de cargos

  // Autocomplete Estado
  termoPesquisaCargo = signal<string>('');
  mostrarDropdownCargos = signal<boolean>(false);

  // 1. Definir o signal 'colaboradores' que faltava
  colaboradores = signal<ColaboradorListDto[]>([]);
  // Estado para o Modal de Transferência
  mostrarModalTransferencia = signal(false);
  gestorParaDesativarId: string | null = null;
  novoGestorId: string | null = null;

  // Feedback visual
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  constructor() {
    this.loggedInUserRole = this.authService.getUserRole();

    // Inicializar o formulário com as validações
    this.colaboradorForm = this.fb.group({
      nomeCompleto: ['', [Validators.required, Validators.minLength(3)]],
      emailPessoal: ['', [Validators.required, Validators.email]],
      nif: ['', [Validators.required, Validators.pattern(/^\d{9}$/)]], // Exatamente 9 dígitos
      instituicaoId: [null, [Validators.required]],

      // Campos opcionais mas com validação de formato se preenchidos
      telemovel: [null, [Validators.pattern(/^[0-9]{9}$/)]],
      salarioBase: [null, [Validators.min(0)]],
      numeroAgente: [null],

      // Outros campos
      morada: [''],
      iban: [''],
      dataNascimento: [null],
      dataAdmissao: [new Date().toISOString().split('T')[0], Validators.required],
      cargo: [null, [Validators.required]],
      tipoContrato: [''],
      departamento: [''],
      localizacao: ['']
    });
  }

  ngOnInit(): void {
    this.carregarColaboradores();
    this.configurarPermissoes();
    this.carregarInstituicoes();
    this.verificarPerfil();
  }

  configurarPermissoes(): void {
    if (this.loggedInUserRole === 'GestorMaster') {
      this.carregarInstituicoes();
    } else if (this.loggedInUserRole === 'GestorRH') {
      // Pré-preencher a instituição e bloquear o campo se necessário
      const instId = this.authService.getInstituicaoId();
      const instNome = this.authService.getInstituicaoNome();

      if (instId) {
        this.colaboradorForm.patchValue({ instituicaoId: instId });
        this.nomeInstituicaoGestor = instNome || '';
      }
    }
  }

  // Computed: Filtra cargos baseado no que o user escreve
  cargosFiltrados = computed(() => {
    const termo = this.termoPesquisaCargo().toLowerCase();
    return this.cargos().filter(c => c.nome.toLowerCase().includes(termo));
  });

  carregarDadosIniciais() {
    // TODO: Substituir por chamada real ao serviço de cargos
    // this.cargoService.getCargos().subscribe(...)
    this.cargos.set([
      { id: 'guid-1', nome: 'Desenvolvedor Senior', instituicao: '...', isAtivo: true },
      { id: 'guid-2', nome: 'Gestor de RH', instituicao: '...', isAtivo: true },
      { id: 'guid-3', nome: 'Assistente Administrativo', instituicao: '...', isAtivo: true }
    ]);
  }

  // --- Lógica do Autocomplete de Cargo ---
  selecionarCargo(cargo: CargoDto) {
    this.colaboradorForm.patchValue({ cargoId: cargo.id }); // Guarda o ID
    this.termoPesquisaCargo.set(cargo.nome); // Mostra o nome no input visual
    this.mostrarDropdownCargos.set(false); // Fecha dropdown
  }

  aoDigitarCargo(event: Event) {
    const input = event.target as HTMLInputElement;
    this.termoPesquisaCargo.set(input.value);
    this.colaboradorForm.patchValue({ cargoId: null }); // Limpa ID se user mudar o texto
    this.mostrarDropdownCargos.set(true);
  }
  // ----------------------------------------

  carregarInstituicoes(): void {
    this.instituicaoService.getInstituicoes().subscribe({
      next: (data) => this.listaInstituicoes = data.filter(i => i.isAtiva),
      error: () => this.mostrarFeedback('Erro ao carregar instituições.', true)
    });
  }

  carregarColaboradores(): void {
    this.colaboradorService.getColaboradores().subscribe({
      next: (data) => this.listaColaboradores = data,
      error: (err) => console.error(err)
    });
  }

  verificarPerfil() {
    this.isMaster.set(this.authService.hasRole('GestorMaster'));

    if (this.isMaster()) {
      // MASTER: Carrega lista e obriga seleção manual
      this.carregarInstituicoes();
      this.colaboradorForm.get('instituicaoId')?.enable();
    } else {
      // RH: Carrega ID automático e "tranca" o campo
      const myInstId = this.authService.getInstituicaoId();

      if (myInstId) {
        this.colaboradorForm.patchValue({ instituicaoId: myInstId });
        // Importante: Não usar disable(), senão o valor não é enviado no submit.
        // Vamos usar 'readonly' no HTML.
      }
    }
  }

  // --- AÇÕES DO FORMULÁRIO ---

  abrirModalNovo(): void {
    this.idColaboradorEmEdicao = '';
    this.limparFeedback();
    this.colaboradorForm.reset(); // Limpa o formulário

    // Define valores padrão
    this.colaboradorForm.patchValue({
      dataAdmissao: new Date().toISOString().split('T')[0]
    });

    // Se for GestorRH, volta a forçar o ID da instituição
    if (this.loggedInUserRole === 'GestorRH') {
      const instId = this.authService.getInstituicaoId();
      if (instId) this.colaboradorForm.patchValue({ instituicaoId: instId });
    }

    this.isModalAberto = true;
  }

  selecionarParaEditar(colaborador: Colaborador): void {
    this.limparFeedback();
    this.idColaboradorEmEdicao = colaborador.id;

    // Buscar detalhes completos à API para preencher o form
    this.colaboradorService.getColaboradorById(colaborador.id).subscribe({
      next: (data) => {
        // O patchValue preenche automaticamente os campos que têm o mesmo nome
        this.colaboradorForm.patchValue({
          nomeCompleto: data.nomeCompleto,
          emailPessoal: data.emailPessoal, // Atenção: no DTO pode vir como 'email' ou 'emailPessoal'
          nif: data.nif,
          instituicaoId: data.instituicaoId,
          numeroAgente: data.numeroAgente,
          telemovel: data.telemovel,
          morada: data.morada,
          iban: data.iban,
          dataNascimento: data.dataNascimento ? data.dataNascimento.split('T')[0] : null,
          dataAdmissao: data.dataAdmissao ? data.dataAdmissao.split('T')[0] : '',
          cargo: data.cargo,
          tipoContrato: data.tipoContrato,
          salarioBase: data.salarioBase,
          departamento: data.departamento,
          localizacao: data.localizacao
        });
        this.isModalAberto = true;
      },
      error: () => this.mostrarFeedback('Erro ao carregar detalhes do colaborador.', true)
    });
  }

  onSubmit(): void {
    this.limparFeedback();

    if (this.colaboradorForm.invalid) {
      // Marca todos os campos como "tocados" para mostrar os erros no HTML
      this.colaboradorForm.markAllAsTouched();
      this.mostrarFeedback('Por favor, corrija os erros no formulário.', true);
      return;
    }

    const dadosParaEnviar = this.colaboradorForm.value;

    // Garantia extra para GestorRH
    if (this.loggedInUserRole === 'GestorRH') {
      dadosParaEnviar.instituicaoId = this.authService.getInstituicaoId();
    }

    if (this.idColaboradorEmEdicao) {
      this.colaboradorService.atualizarColaborador(this.idColaboradorEmEdicao, dadosParaEnviar).subscribe({
        next: (res) => {
          this.mostrarFeedback('Atualizado com sucesso!', false);
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => this.tratarErro(err)
      });
    } else {
      this.colaboradorService.criarColaborador(dadosParaEnviar).subscribe({
        next: (res) => {
          this.mostrarFeedback('Criado com sucesso!', false);
          this.carregarColaboradores();
          this.fecharModal();
        },
        error: (err) => this.tratarErro(err)
      });
    }
  }

  // --- AÇÕES DE LISTAGEM ---

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

  // Lista de possíveis novos gestores (podes filtrar para não mostrar o próprio)
  potenciaisGestores = computed(() => {
    return this.colaboradores().filter(c => c.id !== this.gestorParaDesativarId);
  });

  desativarColaborador(colaborador: Colaborador) {
    if (!confirm(`Deseja desativar ${colaborador.nomeCompleto}?`)) return;

    this.colaboradorService.desativar(colaborador.id).subscribe({
      next: () => {
        alert('Colaborador desativado com sucesso.');
        this.carregarColaboradores();
      },
      error: (err : any) => {
        // Lógica Inteligente: Deteta o erro específico do Backend
        if (err.error?.message === 'HAS_SUBORDINATES' || err.message.includes('HAS_SUBORDINATES')) {

          const desejaTransferir = confirm(
            'Este colaborador é Gestor de uma equipa ativa. ' +
            'Não pode ser desativado sem antes transferir a equipa.\n\n' +
            'Deseja transferir a equipa agora?'
          );

          if (desejaTransferir) {
            this.gestorParaDesativarId = colaborador.id;
            this.mostrarModalTransferencia.set(true); // Abre o Modal (que deves criar no HTML)
          }

        } else {
          alert('Erro ao desativar: ' + err.message);
        }
      }
    });
  }

  confirmarTransferencia() {
    if (!this.gestorParaDesativarId || !this.novoGestorId) return;

    this.colaboradorService.transferirEquipa(this.gestorParaDesativarId, this.novoGestorId)
      .subscribe({
        next: () => {
          alert('Equipa transferida! A desativar o gestor antigo...');
          this.mostrarModalTransferencia.set(false);

          // Agora que já não tem equipa, desativa sem problemas
          this.colaboradorService.desativar(this.gestorParaDesativarId!).subscribe(() => {
            this.carregarColaboradores();
            this.gestorParaDesativarId = null;
            this.novoGestorId = null;
          });
        },
        // 4. CORREÇÃO: Tipar 'err'
        error: (err: any) => alert('Erro na transferência: ' + (err.error?.message || err.message))
      });
  }

  mudarEstado(colaborador: Colaborador): void {
    const acao = colaborador.isAtivo ? "desativar" : "reativar";
    if (!confirm(`${acao} "${colaborador.nomeCompleto}"?`)) return; // Mensagem corrigida

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
    this.idColaboradorEmEdicao = '';
    this.colaboradorForm.reset();
  }

  // --- AUXILIARES ---

  private tratarErro(err: any) {
    const msg = err.error?.message || 'Ocorreu um erro.';
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
    // Opcional: limpar mensagem após 5 segundos
    setTimeout(() => this.feedbackMessage = null, 5000);
  }

  private limparFeedback(): void {
    this.feedbackMessage = null;
    this.isError = false;
  }
}
