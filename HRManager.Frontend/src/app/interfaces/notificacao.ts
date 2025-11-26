export interface Notificacao {
  id: number;
  titulo: string;
  mensagem: string;
  link?: string;
  dataCriacao: string;
  lida: boolean;
}
