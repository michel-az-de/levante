using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Levante.SharedKernel.Infrastructure.Outbox;

/// <summary>
/// Publica eventos num exchange topic duravel do RabbitMQ (mensagens persistentes).
/// Mantem conexao/canal reaproveitados; reconecta se cairem. Usado so pelo relay
/// (loop unico), sem concorrencia.
/// </summary>
internal sealed class PublicadorRabbitMq(IOptions<RabbitMqOptions> opcoes) : IPublicadorDeEventos, IAsyncDisposable
{
    private readonly RabbitMqOptions _opcoes = opcoes.Value;
    private IConnection? _conexao;
    private IChannel? _canal;

    public async Task PublicarAsync(string tipo, Guid eventId, ReadOnlyMemory<byte> corpoJson, CancellationToken ct)
    {
        var canal = await GarantirCanalAsync(ct);

        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = eventId.ToString(),
        };

        await canal.BasicPublishAsync(
            exchange: _opcoes.Exchange,
            routingKey: tipo,
            mandatory: false,
            basicProperties: propriedades,
            body: corpoJson,
            cancellationToken: ct);
    }

    private async Task<IChannel> GarantirCanalAsync(CancellationToken ct)
    {
        if (_canal is { IsOpen: true })
        {
            return _canal;
        }

        var fabrica = new ConnectionFactory
        {
            HostName = _opcoes.Hostname,
            Port = _opcoes.Port,
            UserName = _opcoes.Username,
            Password = _opcoes.Password,
            VirtualHost = _opcoes.VirtualHost,
        };

        _conexao = await fabrica.CreateConnectionAsync(ct);
        _canal = await _conexao.CreateChannelAsync(cancellationToken: ct);
        await _canal.ExchangeDeclareAsync(
            _opcoes.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);

        return _canal;
    }

    public async ValueTask DisposeAsync()
    {
        if (_canal is not null)
        {
            await _canal.DisposeAsync();
        }

        if (_conexao is not null)
        {
            await _conexao.DisposeAsync();
        }
    }
}
