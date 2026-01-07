import { Component, inject, OnInit } from '@angular/core';
import { AusenciaDto } from '../../interfaces/ausenciaDto';
import { AusenciaService } from '../../services/ausencia.service';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AusenciaSaldoDto } from '../../interfaces/ausenciaSaldoDto';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-minhas-ausencias',
  imports: [FormsModule, CommonModule, ReactiveFormsModule],
  templateUrl: './minhas-ausencias.html',
  styleUrl: './minhas-ausencias.css',
})
export class MinhasAusencias implements OnInit {
  private ausenciaService = inject(AusenciaService);
  private fb = inject(FormBuilder);

  public listaAusencias: AusenciaDto[] = [];
  public saldoInfo: AusenciaSaldoDto | null = null;

  // Formulário Reativo
  public ausenciaForm: FormGroup;
  public isModalAberto: boolean = false;
  public ficheiroSelecionado: File | null = null;

  // Feedback
  public feedbackMessage: string | null = null;
  public isError: boolean = false;

  constructor() {
    // Criação do formulário com Validação Personalizada de Datas
    this.ausenciaForm = this.fb.group({
      tipo: ['Ferias', Validators.required],
      dataInicio: [new Date().toISOString().split('T')[0], Validators.required],
      dataFim: [new Date().toISOString().split('T')[0], Validators.required],
      motivo: [''],
      documento: [null]
    }, { validators: this.validarDatas }); // <--- Validador de grupo
  }

  ngOnInit(): void {
    this.carregarAusencias();
    this.carregarSaldo();
  }

  // --- Validador Personalizado: Fim deve ser maior que Início ---
  private validarDatas(group: AbstractControl): ValidationErrors | null {
    const inicio = group.get('dataInicio')?.value;
    const fim = group.get('dataFim')?.value;

    if (inicio && fim && new Date(inicio) > new Date(fim)) {
      return { datasInvalidas: true };
    }
    return null;
  }

  carregarAusencias(): void {
    this.ausenciaService.getAusencias().subscribe({
      next: (data) => this.listaAusencias = data,
      error: () => this.mostrarFeedback('Erro ao carregar histórico.', true)
    });
  }

  carregarSaldo(): void {
    this.ausenciaService.getSaldo().subscribe({
      next: (data) => this.saldoInfo = data
    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.ficheiroSelecionado = file;
    }
  }

  abrirModal(): void {
    this.ausenciaForm.reset({
      tipo: 'Ferias',
      dataInicio: new Date().toISOString().split('T')[0],
      dataFim: new Date().toISOString().split('T')[0]
    });
    this.ficheiroSelecionado = null;
    this.isModalAberto = true;
  }

  fecharModal(): void {
    this.isModalAberto = false;
  }

  onSubmit(): void {
    if (this.ausenciaForm.invalid) return;

    // Preparar objeto para envio
    const request = this.ausenciaForm.value;
    // Adicionar o ficheiro manualmente ao objeto, pois o Reactive Forms não gere ficheiros nativamente bem
    if (this.ficheiroSelecionado) {
      request.documento = this.ficheiroSelecionado;
    }

    this.ausenciaService.solicitarAusencia(request).subscribe({
      next: () => {
        this.mostrarFeedback('Pedido submetido com sucesso!', false);
        this.carregarAusencias();
        this.carregarSaldo();
        this.fecharModal();
      },
      error: (err) => {
        const msg = err.error?.message || 'Erro ao submeter pedido.';
        this.mostrarFeedback(msg, true);
      }
    });
  }

  getDocumentoUrl(caminho: string): string {
    // Usa a URL dinâmica do environment
    return `${environment.baseUrl}/${caminho}`;
  }

  private mostrarFeedback(msg: string, isError: boolean): void {
    this.feedbackMessage = msg;
    this.isError = isError;
    setTimeout(() => this.feedbackMessage = null, 5000);
  }
}
