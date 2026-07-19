namespace Levante.Conteudo.Application.Midias.EnviarMidia;

/// <summary>
/// <paramref name="Conteudo"/> deve ser um stream posicionavel (seekable): o
/// validador confere a assinatura (magic bytes) e reposiciona no inicio antes
/// do handler entregar o stream ao armazenamento.
/// </summary>
public sealed record EnviarMidiaCommand(Stream Conteudo, string ContentType, string NomeArquivo, long Tamanho);
