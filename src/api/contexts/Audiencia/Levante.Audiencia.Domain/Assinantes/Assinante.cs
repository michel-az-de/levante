using Levante.SharedKernel;

namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>
/// Agregado de assinante da newsletter (double opt-in). Nasce <see cref="StatusAssinante.Pendente"/>
/// e so recebe conteudo apos <see cref="Confirmar"/> (clique no link enviado por e-mail).
/// Guarda o e-mail (dado pessoal, finalidade explicita: enviar a newsletter) e a data do
/// consentimento em <see cref="DataCriacao"/> (LGPD, base legal: consentimento). O opt-out
/// e por <see cref="Cancelar"/>. Confirmar/cancelar usam o <see cref="Token"/> opaco.
/// </summary>
public sealed class Assinante
{
    private readonly List<IEventoDeDominio> _eventos = [];

    private Assinante(
        Guid id,
        Email email,
        StatusAssinante status,
        TokenConfirmacao token,
        DateTime dataCriacao,
        DateTime? dataConfirmacao,
        DateTime? dataCancelamento)
    {
        Id = id;
        Email = email;
        Status = status;
        Token = token;
        DataCriacao = dataCriacao;
        DataConfirmacao = dataConfirmacao;
        DataCancelamento = dataCancelamento;
    }

    public Guid Id { get; }

    public Email Email { get; }

    public StatusAssinante Status { get; private set; }

    public TokenConfirmacao Token { get; }

    /// <summary>Momento do consentimento (inicio do double opt-in). Auditoria/LGPD.</summary>
    public DateTime DataCriacao { get; }

    public DateTime? DataConfirmacao { get; private set; }

    public DateTime? DataCancelamento { get; private set; }

    public IReadOnlyList<IEventoDeDominio> Eventos => _eventos;

    /// <summary>
    /// Solicita a assinatura: nasce Pendente com token novo e registra
    /// <see cref="AssinaturaSolicitada"/> (Outbox -> Hiram envia o e-mail de confirmacao).
    /// </summary>
    public static Assinante Solicitar(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var assinante = new Assinante(
            Guid.NewGuid(),
            email,
            StatusAssinante.Pendente,
            TokenConfirmacao.Gerar(),
            DateTime.UtcNow,
            dataConfirmacao: null,
            dataCancelamento: null);

        assinante._eventos.Add(new AssinaturaSolicitada(
            assinante.Id, email.Valor, assinante.Token.Valor, assinante.DataCriacao));
        return assinante;
    }

    /// <summary>Rehidrata um assinante existente (uso da camada de persistencia).</summary>
    public static Assinante Reconstituir(
        Guid id,
        Email email,
        StatusAssinante status,
        TokenConfirmacao token,
        DateTime dataCriacao,
        DateTime? dataConfirmacao,
        DateTime? dataCancelamento) =>
        new(id, email, status, token, dataCriacao, dataConfirmacao, dataCancelamento);

    /// <summary>
    /// Confirma o double opt-in (idempotente; so transiciona a partir de Pendente).
    /// Registra <see cref="AssinanteConfirmado"/>.
    /// </summary>
    public void Confirmar()
    {
        if (Status != StatusAssinante.Pendente)
        {
            return;
        }

        Status = StatusAssinante.Confirmado;
        DataConfirmacao = DateTime.UtcNow;
        _eventos.Add(new AssinanteConfirmado(Id, Email.Valor, DataConfirmacao.Value));
    }

    /// <summary>Cancela a assinatura (opt-out; idempotente). Registra <see cref="AssinaturaCancelada"/>.</summary>
    public void Cancelar()
    {
        if (Status == StatusAssinante.Cancelado)
        {
            return;
        }

        Status = StatusAssinante.Cancelado;
        DataCancelamento = DateTime.UtcNow;
        _eventos.Add(new AssinaturaCancelada(Id, DataCancelamento.Value));
    }

    /// <summary>Limpa eventos ja despachados (chamado apos persistir no Outbox).</summary>
    public void LimparEventos() => _eventos.Clear();
}
