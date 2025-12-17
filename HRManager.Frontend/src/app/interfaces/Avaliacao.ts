import { AvaliacaoItem } from "./AvaliacaoItem";

export interface Avaliacao {
  id: string; // Era number
    cicloId: string; // Era number
    colaboradorId: string; // Era number
    gestorId: string; // Era number

    // Propriedades de Display (precisam existir porque o DTO envia)
    nomeColaborador: string;
    nomeGestor: string;
    nomeCiclo: string;

    estado: 'NaoIniciada' | 'EmAndamento' | 'AutoAvaliacao' | 'AvaliacaoGestor' | 'ReuniaoFeedback' | 'Finalizada';
    mediaFinal?: number;
    comentarioFinalGestor?: string;
    dataCriacao: Date;
    dataConclusao?: Date;
    itens: AvaliacaoItem[];
}
