using Levante.Conteudo.Application.Artigos.CriarArtigo;
using Levante.Conteudo.Domain.Artigos;
using Shouldly;
using Xunit;

namespace Levante.Conteudo.UnitTests.Artigos;

/// <summary>
/// Gerador de postagem "em todos os formatos" na camada de dominio/handler (roda sem Mongo).
/// Prova que o corpo do artigo — markdown, HTML cru, imagem de midia, unicode, etc. — atravessa
/// o handler preservado byte a byte (integridade) e nasce em rascunho, e que corpo vazio/branco
/// e barrado pela validacao. O round-trip ponta-a-ponta (via HTTP + Mongo) fica na integracao.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ArtigoFormatosTests
{
    private static CriarArtigoCommandHandler Handler(ArtigoRepositorioEmMemoria repo) =>
        new(repo, new CriarArtigoCommandValidator(new CategoriaRepositorioEmMemoria()));

    /// <summary>Cada formato de conteudo que o corpo do artigo aceita hoje (string livre, markdown pretendido).</summary>
    public static IEnumerable<object[]> FormatosDeConteudo()
    {
        // Chars unicode montados por code point para manter o .cs em ASCII (convencao do repo) sem
        // depender do encoding do arquivo; em runtime e uma string unicode de verdade (acentos, emoji, CJK).
        var unicode = "Cora" + (char)0x00E7 + (char)0x00E3 + "o " + (char)0x00E0 + " noite: "
            + (char)0x00E7 + ", " + (char)0x00E3 + ", " + (char)0x00E9 + " - emoji "
            + char.ConvertFromUtf32(0x1F600) + " - CJK " + (char)0x4E2D + (char)0x6587 + " - integridade.";

        return
        [
            ["texto-puro", "Um paragrafo simples, sem marcacao."],
            ["headings", "# H1\n\n## H2\n\n### H3\n\nCorpo."],
            ["listas", "- item a\n- item b\n\n1. um\n2. dois"],
            ["tabela-gfm", "| a | b |\n| - | - |\n| 1 | 2 |"],
            ["task-list", "- [ ] pendente\n- [x] feito"],
            ["code-fence", "```csharp\nvar x = 1;\n```\n\nInline `code` aqui."],
            ["blockquote", "> uma citacao\n> em duas linhas"],
            ["links", "Veja [o guia](https://exemplo.com) e https://autolink.dev"],
            ["imagem-midia-relativa", "Antes\n\n![diagrama](/midias/2f1c9b6e-0000-0000-0000-000000000000)\n\nDepois"],
            ["imagem-url-absoluta", "![capa](https://cdn.exemplo.com/capa.png)"],
            ["html-cru-embutido", "Texto\n\n<div class=\"callout\">bloco cru</div>\n\n<script>alert(1)</script>"],
            ["unicode-acentos-emoji", unicode],
            ["markdown-rico", "# Titulo\n\n> nota\n\n- a\n- b\n\n```\ncodigo\n```\n\n| x | y |\n| - | - |\n| 1 | 2 |"],
        ];
    }

    [Theory]
    [MemberData(nameof(FormatosDeConteudo))]
    public async Task Criar_conteudoEmVariosFormatos_preservaOConteudoEnasceRascunho(string nome, string conteudo)
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Handler(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand($"Formato {nome}", $"formato-{nome}", "Resumo.", conteudo),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldNotBeNull();
        resultado.Valor.Conteudo.ShouldBe(conteudo); // integridade: nada e reescrito nem sanitizado no dominio
        resultado.Valor.Status.ShouldBe(nameof(StatusArtigo.Rascunho));
        repo.Adicionados.ShouldBe(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task Criar_conteudoVazioOuEmBranco_falhaComValidacao(string conteudo)
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Handler(repo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "conteudo-invalido", "Resumo.", conteudo),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
        repo.Adicionados.ShouldBe(0);
    }

    /// <summary>
    /// Teto do Conteudo (issue #101): ate 200k chars e aceito; acima disso a validacao barra.
    /// Defende o documento Mongo (16MB) sem atrapalhar artigos longos.
    /// </summary>
    [Fact]
    public async Task Criar_conteudoNoLimiteDe200k_eAceito()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Handler(repo);
        var noLimite = new string('a', Artigo.TamanhoMaximoConteudo);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "conteudo-no-limite", "Resumo.", noLimite),
            CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor!.Conteudo.Length.ShouldBe(Artigo.TamanhoMaximoConteudo);
    }

    [Fact]
    public async Task Criar_conteudoAcimaDoLimite_falhaComValidacao()
    {
        var repo = new ArtigoRepositorioEmMemoria();
        var handler = Handler(repo);
        var acima = new string('a', Artigo.TamanhoMaximoConteudo + 1);

        var resultado = await handler.Handle(
            new CriarArtigoCommand("Titulo", "conteudo-grande-demais", "Resumo.", acima),
            CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("validacao");
        repo.Adicionados.ShouldBe(0);
    }
}
