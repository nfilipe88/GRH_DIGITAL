export interface CriarAusenciaRequest {
  tipo: string;
  dataInicio: string; // YYYY-MM-DD
  dataFim: string;    // YYYY-MM-DD
  motivo?: string;
  // O ficheiro não entra na interface JSON direta, será tratado à parte
  documento?: File; // Opcional (apenas para tipagem interna no componente)
}
