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

        var id = Guid.NewGuid();
        var salva = await armazenamento.SalvarAsync(id, comando.Conteudo, comando.ContentType, comando.NomeArquivo, ct);

        return Result.Ok(MidiaResponse.DeMidia(salva));
    }
}
