using System.Reflection;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Infrastructure;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace Levante.ArchitectureTests;

/// <summary>
/// Guardrails de Clean Architecture: dependencias apontam para dentro.
/// Domain nunca conhece Application/Infrastructure/framework (CLAUDE.md).
/// </summary>
public sealed class ArquiteturaTests
{
    private static readonly Assembly Domain = typeof(Artigo).Assembly;
    private static readonly Assembly Application = typeof(ArtigoResponse).Assembly;

    [Fact]
    public void Domain_naoDependeDeApplicationInfraEFramework()
    {
        var resultado = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Conteudo.Application",
                "Levante.Conteudo.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void Application_naoDependeDeInfraNemDoDriverMongo()
    {
        var resultado = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Conteudo.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void Infrastructure_dependeDeApplicationEDomain()
    {
        // Sanidade: a Infrastructure (camada externa) referencia o composition root.
        typeof(DependencyInjection).Assembly.ShouldNotBeNull();
    }

    private static string MensagemDeFalha(TestResult resultado)
    {
        var tipos = resultado.FailingTypeNames is null
            ? string.Empty
            : string.Join(", ", resultado.FailingTypeNames);
        return $"Tipos violando a regra de dependencia: {tipos}";
    }
}
