import { CertificacaoDto } from "./certificacaoDto";
import { HabilitacaoDto } from "./habilitacaoDto";

export interface PerfilDto {
  colaboradorId: number;
  nomeCompleto: string;
  cargo: string;
  email: string;
  habilitacoes: HabilitacaoDto[];
  certificacoes: CertificacaoDto[];
}
