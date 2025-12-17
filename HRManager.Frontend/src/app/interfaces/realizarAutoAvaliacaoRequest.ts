import { ItemResposta } from "./itemResposta";

export interface RealizarAutoAvaliacaoRequest {
  respostas: ItemResposta[];
  finalizar: boolean;
}
