using FluentValidation;

namespace Levante.Conteudo.Application.Midias.EnviarMidia;

public sealed class EnviarMidiaCommandValidator : AbstractValidator<EnviarMidiaCommand>
{
    /// <summary>Tamanho maximo aceito pelo dominio (defesa em profundidade: o endpoint tambem limita via Kestrel).</summary>
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024;

    private static readonly string[] TiposPermitidos = ["image/png", "image/jpeg", "image/webp", "image/gif"];

    public EnviarMidiaCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(tipo => TiposPermitidos.Contains(tipo))
            .WithMessage("Tipo de midia nao suportado. Use PNG, JPEG, WEBP ou GIF.");

        RuleFor(x => x.Tamanho)
            .GreaterThan(0)
            .LessThanOrEqualTo(TamanhoMaximoBytes)
            .WithMessage($"Midia excede o tamanho maximo de {TamanhoMaximoBytes / (1024 * 1024)}MB.");

        RuleFor(x => x)
            .MustAsync((comando, ct) => AssinaturaDeMidia.ConfereAsync(comando.Conteudo, comando.ContentType, ct))
            .WithName("Conteudo")
            .WithMessage("O conteudo do arquivo nao corresponde ao tipo declarado.")
            // So confere a assinatura se o tipo declarado ja e um dos permitidos:
            // evita duas mensagens de erro redundantes para o mesmo motivo.
            .When(x => TiposPermitidos.Contains(x.ContentType));
    }
}
