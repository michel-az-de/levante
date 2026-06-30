using Levante.Conteudo.Domain.Artigos;

namespace Levante.Conteudo.Application.Artigos;

/// <summary>Converte as tags (strings) do comando em VOs <see cref="Tag"/> (validadas no validator), sem duplicatas.</summary>
internal static class TagsDoComando
{
    public static IReadOnlyList<Tag> Converter(IReadOnlyList<string>? tags) =>
        [.. (tags ?? []).Select(t => new Tag(t)).Distinct()];
}
