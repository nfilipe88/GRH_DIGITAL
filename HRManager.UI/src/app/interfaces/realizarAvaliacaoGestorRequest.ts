import { ItemResposta } from "./itemResposta";


export interface RealizarAvaliacaoGestorRequest {
  respostas: ItemResposta[];
  comentarioFinal: string;
  finalizar: boolean;
}
