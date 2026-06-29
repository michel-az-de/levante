using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Levante.SharedKernel;

namespace Levante.Identity.Application.Autenticacao;

/// <summary>
/// Login do admin (handler chamado direto, GAP-F). Aplica lockout e NAO enumera
/// usuario (falha generica). Hashing/JWT via ports. Rate limit fica no endpoint.
/// </summary>
public sealed class AutenticarCommandHandler(
    IAdministradorRepository repositorio,
    IHashDeSenha hashDeSenha,
    IGeradorDeToken geradorDeToken)
{
    private static readonly Error CredenciaisInvalidas =
        new("credenciais_invalidas", "Credenciais invalidas.");

    public async Task<Result<TokenDeAcessoResponse>> Handle(AutenticarCommand comando, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        if (!Email.TryParse(comando.Email, out var email) || email is null)
        {
            return Result.Falha<TokenDeAcessoResponse>(CredenciaisInvalidas);
        }

        var administrador = await repositorio.GetByEmailAsync(email.Valor, ct);
        if (administrador is null || !administrador.Ativo)
        {
            return Result.Falha<TokenDeAcessoResponse>(CredenciaisInvalidas);
        }

        if (administrador.EstaBloqueado(DateTime.UtcNow))
        {
            return Result.Falha<TokenDeAcessoResponse>(
                new Error("conta_bloqueada", "Conta temporariamente bloqueada. Tente novamente mais tarde."));
        }

        if (!hashDeSenha.Verificar(administrador.SenhaHash, comando.Senha))
        {
            administrador.RegistrarFalhaDeLogin();
            await repositorio.UpdateAsync(administrador, ct);
            return Result.Falha<TokenDeAcessoResponse>(CredenciaisInvalidas);
        }

        administrador.ResetarFalhas();
        await repositorio.UpdateAsync(administrador, ct);

        return Result.Ok(geradorDeToken.Gerar(administrador));
    }
}
