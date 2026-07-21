namespace Levante.Conteudo.Application.Midias.EnviarMidia;

/// <summary>
/// <paramref name="Conteudo"/> deve ser um stream posicionavel (seekable): o
/// validador confere a assinatura (magic bytes) lendo do inicio e devolve o cursor
/// para onde estava, e o handler rebobina para 0 antes de gravar. Stream nao
/// posicionavel e recusado na validacao (nao da para conferir a assinatura).
/// <paramref name="Tamanho"/> e o tamanho declarado pelo transporte, usado so para
/// recusar cedo; o tamanho autoritativo e o que o armazenamento reporta ter gravado.
/// </summary>
public sealed record EnviarMidiaCommand(Stream Conteudo, string ContentType, string NomeArquivo, long Tamanho);
