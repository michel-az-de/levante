namespace Levante.Engajamento.Application.Reacoes.ObterReacoesDoArtigo;

/// <summary>Contagens de reacao de um artigo. <paramref name="Visitante"/> vazio = sem "minhas".</summary>
public sealed record ObterReacoesDoArtigoQuery(Guid ArtigoId, string Visitante);
