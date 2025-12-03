using FluentValidation;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Validators
{
    public class CriarAusenciaValidator : AbstractValidator<CriarAusenciaRequest>
    {
        // Precisamos do contexto para validações complexas de banco de dados (ex: conflito de datas)
        private readonly HRManagerDbContext _context;
        private readonly string _userEmail;

        // Nota: Para injetar serviços no Validator, a configuração no Program.cs precisa ser cuidadosa,
        // mas para regras simples (datas) não precisamos do contexto. 
        // Para regras de conflito (Query ao banco), o ideal é fazer no Service layer OU usar validação assíncrona.

        public CriarAusenciaValidator()
        {
            // Validação do Tipo
            RuleFor(x => x.Tipo)
                .IsInEnum().WithMessage("O tipo de ausência selecionado é inválido.");

            // Validação da Data de Início
            RuleFor(x => x.DataInicio)
                .NotEmpty().WithMessage("A data de início é obrigatória.")
                // Nota: Usamos DateTime.Today para ignorar a hora atual, focando apenas no dia
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Não é possível solicitar ausências para datas passadas.");

            // Validação da Data de Fim
            RuleFor(x => x.DataFim)
                .NotEmpty().WithMessage("A data de fim é obrigatória.")
                .GreaterThanOrEqualTo(x => x.DataInicio).WithMessage("A data de fim deve ser igual ou posterior à data de início.");

            // Regra Condicional: Se NÃO for Férias, exige Motivo
            RuleFor(x => x.Motivo)
                .NotEmpty()
                .When(x => x.Tipo != TipoAusencia.Ferias)
                .WithMessage("É obrigatório indicar o motivo para este tipo de ausência.");

            // Validação de Ficheiro (Opcional - Exemplo)
            // Se quiseres limitar o tamanho ou tipo do anexo:
            RuleFor(x => x.Documento)
                .Must(doc => doc == null || doc.Length <= 5 * 1024 * 1024) // Max 5MB
                .WithMessage("O documento anexo não pode exceder 5MB.")
                .When(x => x.Documento != null);
        }
    }
}
