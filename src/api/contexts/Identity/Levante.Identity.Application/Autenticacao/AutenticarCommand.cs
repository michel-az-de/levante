namespace Levante.Identity.Application.Autenticacao;

/// <summary>Comando de login do administrador.</summary>
public sealed record AutenticarCommand(string Email, string Senha);
