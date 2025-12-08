import { AvaliacaoItem } from "./AvaliacaoItem";

export interface Avaliacao {
  id: number;
  nomeColaborador: string;
  nomeCiclo: string;
  dataConclusao: string;
  estado: string;
  notaFinal?: number;
  itens: AvaliacaoItem[];
  nomeGestor: string;
  comentarioFinalGestor?: string;
}
