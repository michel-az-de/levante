using FluentValidation.Results;
using Levante.SharedKernel;

namespace Levante.Audiencia.Application;

/// <summary>
/// Converte falhas do FluentValidation em <see cref="Error"/> (codigo "validacao",
/// mapeado para 400 no endpoint). Mantem o fluxo de negocio sem exception.
/// </summary>
internal static class ErroDeValidacao
{
    public const string Codigo = "validacao";

    public static Error De(ValidationResult resultado) =>
        Error.Validacao(Codigo, string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage)));
}
