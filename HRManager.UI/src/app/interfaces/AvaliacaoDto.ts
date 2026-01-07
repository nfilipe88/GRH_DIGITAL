import { AvaliacaoItemDto } from "./AvaliacaoItemDto";
import { CicloAvaliacaoDto } from "./cicloAvaliacaoDto";

export interface AvaliacaoDto {
  id: string; // Era number
    cicloId: string; // Era number
    colaboradorId: string; // Era number
    gestorId: string; // Era number

    // Propriedades de Display (precisam existir porque o DTO envia)
    nomeColaborador: string;
    nomeGestor: string;
    // nomeCiclo: string;

    estado: 'NaoIniciada' | 'EmAndamento' | 'AutoAvaliacaoPendente' | 'AvaliacaoGestor' | 'ReuniaoFeedback' | 'Finalizada';
    mediaFinal: number;
    comentarioFinalGestor?: string;
    dataCriacao: Date;
    dataConclusao?: Date;
    itens: AvaliacaoItemDto[];
    ciclo: CicloAvaliacaoDto;
}
