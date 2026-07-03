namespace Levante.Audiencia.Domain.Assinantes;

/// <summary>
/// Lancada quando o e-mail ja esta cadastrado (violacao do indice unico). A
/// Application trata como idempotente e NAO revela que o e-mail ja existe
/// (privacidade/LGPD: a resposta publica e sempre a mesma).
/// </summary>
public sealed class AssinanteJaExisteException(string email)
    : Exception($"Ja existe assinante para '{email}'.")
{
    public string Email { get; } = email;
}
