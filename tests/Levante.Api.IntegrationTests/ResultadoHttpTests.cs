using Levante.Api.Endpoints;
using Levante.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shouldly;
using Xunit;

namespace Levante.Api.IntegrationTests;

/// <summary>
/// Contrato de erro da borda: o status HTTP e dirigido pelo TIPO do Error,
/// nao por tabela de codigos. Teste de logica pura (sem container).
/// </summary>
[Trait("Category", "Unit")]
public sealed class ResultadoHttpTests
{
    [Fact]
    public void Nao_encontrado_vira_404()
    {
        var resultado = ResultadoHttp.Falha(Error.NaoEncontrado("artigo_nao_encontrado", "Artigo nao encontrado."));

        resultado.ShouldBeOfType<NotFound>();
    }

    [Theory]
    [InlineData("slug_em_uso")]
    [InlineData("transicao_invalida")]
    public void Conflito_vira_409_com_problem_details(string codigo)
    {
        var resultado = ResultadoHttp.Falha(Error.Conflito(codigo, "Conflito de estado."));

        var problema = resultado.ShouldBeOfType<ProblemHttpResult>();
        problema.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problema.ProblemDetails.Detail.ShouldBe("Conflito de estado.");
    }

    [Fact]
    public void Validacao_vira_400_com_problem_details()
    {
        var resultado = ResultadoHttp.Falha(Error.Validacao("validacao", "Titulo obrigatorio."));

        var problema = resultado.ShouldBeOfType<ProblemHttpResult>();
        problema.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problema.ProblemDetails.Detail.ShouldBe("Titulo obrigatorio.");
    }

    [Fact]
    public void Falha_nao_classificada_vira_500_para_aparecer_cedo()
    {
        var resultado = ResultadoHttp.Falha(new Error("qualquer_codigo_novo", "Falha sem classificacao."));

        var problema = resultado.ShouldBeOfType<ProblemHttpResult>();
        problema.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }
}
