namespace Levante.Conteudo.Domain.Midias;

/// <summary>
/// Midia aberta para leitura. O chamador e responsavel por dispor
/// <see cref="Conteudo"/> (a Infrastructure entrega o stream de download aberto).
/// </summary>
public sealed record MidiaConteudo(Stream Conteudo, string ContentType, long Tamanho);
