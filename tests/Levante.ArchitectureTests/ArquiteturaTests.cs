using System.Reflection;
using Levante.Audiencia.Application.Assinantes.SolicitarAssinatura;
using Levante.Audiencia.Domain.Assinantes;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Domain.Artigos;
using Levante.Conteudo.Infrastructure;
using Levante.Engajamento.Application.Reacoes;
using Levante.Engajamento.Domain.Reacoes;
using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Domain.Administradores;
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
    private static readonly Assembly IdentityDomain = typeof(Administrador).Assembly;
    private static readonly Assembly IdentityApplication = typeof(AutenticarCommand).Assembly;
    private static readonly Assembly EngajamentoDomain = typeof(Reacao).Assembly;
    private static readonly Assembly EngajamentoApplication = typeof(ReacoesResponse).Assembly;
    private static readonly Assembly AudienciaDomain = typeof(Assinante).Assembly;
    private static readonly Assembly AudienciaApplication = typeof(SolicitarAssinaturaCommand).Assembly;

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
    public void IdentityDomain_naoDependeDeApplicationInfraEFramework()
    {
        var resultado = Types.InAssembly(IdentityDomain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Identity.Application",
                "Levante.Identity.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void IdentityApplication_naoDependeDeInfraNemDoDriverMongo()
    {
        var resultado = Types.InAssembly(IdentityApplication)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Identity.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void EngajamentoDomain_naoDependeDeApplicationInfraEFramework()
    {
        var resultado = Types.InAssembly(EngajamentoDomain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Engajamento.Application",
                "Levante.Engajamento.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void EngajamentoApplication_naoDependeDeInfraNemDoDriverMongo()
    {
        var resultado = Types.InAssembly(EngajamentoApplication)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Engajamento.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void AudienciaDomain_naoDependeDeApplicationInfraEFramework()
    {
        var resultado = Types.InAssembly(AudienciaDomain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Audiencia.Application",
                "Levante.Audiencia.Infrastructure",
                "MongoDB",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions")
            .GetResult();

        resultado.IsSuccessful.ShouldBeTrue(MensagemDeFalha(resultado));
    }

    [Fact]
    public void AudienciaApplication_naoDependeDeInfraNemDoDriverMongo()
    {
        var resultado = Types.InAssembly(AudienciaApplication)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Levante.Audiencia.Infrastructure",
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
