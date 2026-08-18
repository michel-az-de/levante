using FluentValidation.Results;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application;

/// <summary>
/// Converte falhas do FluentValidation em <see cref="Error"/> (codigo "validacao",
/// mapeado para 400 no endpoint). Mantem o fluxo de negocio sem exception.
/// Transversal ao contexto Conteudo (Artigos, Categorias, Midias) — por isso vive
/// na raiz do .Application, como nos demais contextos (Audiencia, Engajamento).
/// </summary>
internal static class ErroDeValidacao
{
    public const string Codigo = "validacao";

    public static Error De(ValidationResult resultado) =>
        Error.Validacao(Codigo, string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage)));
}
