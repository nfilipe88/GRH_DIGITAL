import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ChangePasswordRequest } from '../../interfaces/change-password-request';

@Component({
  selector: 'app-alterarpassword',
  imports: [CommonModule, FormsModule],
  templateUrl: './alterarpassword.html',
  styleUrl: './alterarpassword.css',
})
export class Alterarpassword {
  // Modelo para o formulário
  model: ChangePasswordRequest = {
    email: '',
    passwordAtual: '',
    novaPassword: '',
    confirmarNovaPassword: ''
  };

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Tenta recuperar o email passado pelo Login (ver Passo 5)
    const nav = this.router.getCurrentNavigation();
    if (nav?.extras.state && nav.extras.state['email']) {
      this.model.email = nav.extras.state['email'];
    }
  }

  onSubmit() {
    // Validação simples
    if (this.model.novaPassword !== this.model.confirmarNovaPassword) {
      this.errorMessage = 'A nova password e a confirmação não coincidem.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.changePassword(this.model).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Password alterada com sucesso! A redirecionar...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.isLoading = false;
        console.error(err);
        this.errorMessage = err.error?.message || 'Erro ao alterar password.';
      }
    });
  }
}
