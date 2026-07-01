using Levante.SharedKernel;

namespace Levante.Api.Endpoints;

/// <summary>
/// Mapeia o TIPO do erro de negocio (Result) para o status HTTP adequado.
/// Contextos novos nao mexem aqui: basta classificar o Error na Application
/// (Error.Validacao/NaoEncontrado/Conflito). Falha nao classificada vira 500
/// de proposito, para aparecer cedo em teste em vez de fingir 400.
/// </summary>
internal static class ResultadoHttp
{
    public static IResult Falha(Error erro) => erro.Tipo switch
    {
        TipoErro.NaoEncontrado => Results.NotFound(),
        TipoErro.Conflito => Results.Problem(
            detail: erro.Mensagem, statusCode: StatusCodes.Status409Conflict, title: "Conflito"),
        TipoErro.Validacao => Results.Problem(
            detail: erro.Mensagem, statusCode: StatusCodes.Status400BadRequest, title: "Validacao"),
        _ => Results.Problem(
            detail: erro.Mensagem, statusCode: StatusCodes.Status500InternalServerError, title: "Erro"),
    };
}
