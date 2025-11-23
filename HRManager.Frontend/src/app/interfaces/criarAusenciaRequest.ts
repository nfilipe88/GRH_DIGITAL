export interface CriarAusenciaRequest {
  tipo: string;
  dataInicio: string; // YYYY-MM-DD
  dataFim: string;    // YYYY-MM-DD
  motivo?: string;
}
