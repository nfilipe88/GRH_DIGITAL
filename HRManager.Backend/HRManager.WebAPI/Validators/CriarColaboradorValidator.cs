using FluentValidation;
using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Validators
{
    public class CriarColaboradorValidator : AbstractValidator<CriarColaboradorRequest>
    {
        public CriarColaboradorValidator()
        {
            RuleFor(x => x.NomeCompleto)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.");

            RuleFor(x => x.EmailPessoal)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("Formato de email inválido.");

            RuleFor(x => x.NIF)
                .NotEmpty().WithMessage("O NIF é obrigatório.")
                .Matches(@"^\d{9}$").WithMessage("O NIF deve conter exatamente 9 dígitos."); // Exemplo PT

            RuleFor(x => x.SalarioBase)
                .GreaterThan(0).WithMessage("O salário base deve ser maior que zero.");

            RuleFor(x => x.DataAdmissao)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("A data de admissão não pode ser futura (exceto pré-admissão).");
        }
    }
}
