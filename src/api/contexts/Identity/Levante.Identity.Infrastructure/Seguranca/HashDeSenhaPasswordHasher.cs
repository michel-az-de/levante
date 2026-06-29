using Levante.Identity.Application.Ports;
using Microsoft.AspNetCore.Identity;

namespace Levante.Identity.Infrastructure.Seguranca;

/// <summary>
/// Implementacao de IHashDeSenha com o PasswordHasher do framework
/// (PBKDF2-SHA256). O parametro de usuario do hasher e ignorado pela impl padrao.
/// </summary>
internal sealed class HashDeSenhaPasswordHasher : IHashDeSenha
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Contexto = new();

    public string Hash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);
        return Hasher.HashPassword(Contexto, senha);
    }

    public bool Verificar(string hash, string senha)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrEmpty(senha))
        {
            return false;
        }

        return Hasher.VerifyHashedPassword(Contexto, hash, senha) != PasswordVerificationResult.Failed;
    }
}
