export interface Notificacao {
  id: string;
  titulo: string;
  mensagem: string;
  link?: string;
  dataCriacao: Date;
  lida: boolean;
}
