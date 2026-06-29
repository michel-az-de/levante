namespace Levante.Identity.Application.Autenticacao;

/// <summary>Token de acesso emitido no login (JWT bearer). Contrato consumido pelo admin.</summary>
public sealed record TokenDeAcessoResponse(string AccessToken, int ExpiraEmSegundos);
