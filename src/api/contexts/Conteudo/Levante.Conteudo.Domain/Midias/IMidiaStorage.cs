namespace Levante.Conteudo.Domain.Midias;

/// <summary>
/// Porta de armazenamento de midia (blob). A implementacao (GridFS sobre o
/// mesmo Mongo do contexto) fica na Infrastructure; o dominio nao conhece o driver.
/// </summary>
public interface IMidiaStorage
{
    /// <summary>Salva o conteudo sob o id informado (gerado pelo chamador).</summary>
    Task<MidiaArmazenada> SalvarAsync(
        Guid id, Stream conteudo, string contentType, string nomeArquivo, CancellationToken ct);

    /// <summary>Abre a midia para leitura, ou <c>null</c> se o id nao existir.</summary>
    Task<MidiaConteudo?> AbrirAsync(Guid id, CancellationToken ct);
}
