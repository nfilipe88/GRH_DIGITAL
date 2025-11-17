import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
// Dados do formulário
  public credentials = {
    email: '',
    password: '',
  };

  // Feedback de erro
  public errorMessage: string | null = null;
  public isLoading: boolean = false;

  private authService = inject(AuthService);
  private router = inject(Router);

  /**
   * Chamado quando o formulário é submetido
   */
  public onSubmit(): void {
    if (!this.credentials.email || !this.credentials.password) {
      this.errorMessage = 'Por favor, preencha o email e a senha.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.credentials).subscribe({
      next: () => {
        this.isLoading = false;
        // SUCESSO! Redirecionar para a página principal da app
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        // FALHA! Mostrar a mensagem de erro da API
        this.errorMessage = err.error?.message || 'Credenciais inválidas. Tente novamente.';
      },
    });
  }
}
