using FluentValidation;
using Levante.Conteudo.Application.Artigos;
using Levante.Conteudo.Domain.Midias;
using Levante.SharedKernel;

namespace Levante.Conteudo.Application.Midias.EnviarMidia;

public sealed class EnviarMidiaCommandHandler(IMidiaStorage armazenamento, IValidator<EnviarMidiaCommand> validador)
{
    public async Task<Result<MidiaResponse>> Handle(EnviarMidiaCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var validacao = await validador.ValidateAsync(comando, ct);
        if (!validacao.IsValid)
        {
            return Result.Falha<MidiaResponse>(ErroDeValidacao.De(validacao));
        }

        // Rebobina antes de gravar: o validador leu os magic bytes e devolveu o cursor
        // para onde estava, entao um chamador que entregue o stream ja consumido (ex.:
        // MemoryStream logo apos CopyToAsync) gravaria um arquivo de 0 byte com 201 ok.
        if (comando.Conteudo.CanSeek)
        {
            comando.Conteudo.Position = 0;
        }

        // Normaliza o content-type: o header pode chegar com caixa diferente ou com
        // parametros ("image/PNG; charset=binary") e nao queremos servir isso de volta.
        var contentType = TipoDeMidia.Resolver(comando.ContentType)?.ContentType ?? comando.ContentType;

        var id = Guid.NewGuid();
        var salva = await armazenamento.SalvarAsync(id, comando.Conteudo, contentType, comando.NomeArquivo, ct);

        return Result.Ok(MidiaResponse.DeMidia(salva));
    }
}
