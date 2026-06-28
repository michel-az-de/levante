namespace Levante.Conteudo.Domain.Artigos;

/// <summary>
/// Excecao de dominio para artigo inexistente. Usada fora do fluxo de leitura
/// publica da Fatia 0 (o caso de uso de busca por slug vem na Fatia 1).
/// </summary>
public sealed class ArtigoNaoEncontradoException(string slug)
    : Exception($"Artigo com slug '{slug}' nao encontrado.");
