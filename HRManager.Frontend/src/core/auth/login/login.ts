import { Component } from '@angular/core';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  username='';
  password='';
  errorMessage='';

  constructor(private authService: AuthService) {}

  onSubmit(): void {
    this.authService.login(this.username, this.password).subscribe({
      next: (response) => {
        // Sucesso: Navegar para o Dashboard Global (CU-05)
        console.log('Login bem-sucedido. Role:', response.role);
        // Ex: this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.errorMessage = 'Falha no login. Verifique as credenciais.';
        console.error('Erro de login:', err);
      }
    });
  }
}
