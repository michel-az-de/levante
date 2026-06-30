namespace Levante.Conteudo.Application.Artigos.ListarArtigosPublicadosPorCategoria;

/// <summary>Query CQRS-lite (pública): lista os artigos publicados de uma categoria (por slug).</summary>
public sealed record ListarArtigosPublicadosPorCategoriaQuery(string Slug);
