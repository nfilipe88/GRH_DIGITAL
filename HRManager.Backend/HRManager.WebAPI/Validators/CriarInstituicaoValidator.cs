using FluentValidation;
using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Validators
{
    public class CriarInstituicaoValidator : AbstractValidator<CriarInstituicaoRequest>
    {
        public CriarInstituicaoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome da instituição é obrigatório.")
                .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");

            RuleFor(x => x.IdentificadorUnico)
                .NotEmpty().WithMessage("O identificador único (slug) é obrigatório.")
                .Matches("^[a-z0-9-]+$").WithMessage("O identificador deve conter apenas letras minúsculas, números e hífens (ex: 'minha-empresa').");

            RuleFor(x => x.NIF)
                .NotEmpty().WithMessage("O NIF é obrigatório.");
        }
    }
}
