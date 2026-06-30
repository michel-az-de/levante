using Levante.SharedKernel;

namespace Levante.Api.Endpoints;

/// <summary>Mapeia o codigo de erro de negocio (Result) para o status HTTP adequado.</summary>
internal static class ResultadoHttp
{
    public static IResult Falha(Error erro) => erro.Codigo switch
    {
        "artigo_nao_encontrado" or "categoria_nao_encontrada" => Results.NotFound(),
        "slug_em_uso" or "transicao_invalida" => Results.Problem(
            detail: erro.Mensagem, statusCode: StatusCodes.Status409Conflict, title: "Conflito"),
        "validacao" => Results.Problem(
            detail: erro.Mensagem, statusCode: StatusCodes.Status400BadRequest, title: "Validacao"),
        _ => Results.Problem(detail: erro.Mensagem, statusCode: StatusCodes.Status400BadRequest),
    };
}
