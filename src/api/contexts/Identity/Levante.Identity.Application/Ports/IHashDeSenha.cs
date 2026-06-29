namespace Levante.Identity.Application.Ports;

/// <summary>Porta de hashing de senha (implementada na Infrastructure com PasswordHasher).</summary>
public interface IHashDeSenha
{
    string Hash(string senha);

    bool Verificar(string hash, string senha);
}
