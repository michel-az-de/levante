namespace Levante.Conteudo.Domain.Midias;

/// <summary>Metadados de uma midia recem-salva no armazenamento.</summary>
public sealed record MidiaArmazenada(Guid Id, string ContentType, long Tamanho);
