using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Domain.Administradores;

namespace Levante.Identity.Application.Ports;

/// <summary>Porta de geracao de token de acesso (implementada na Infrastructure com JWT).</summary>
public interface IGeradorDeToken
{
    TokenDeAcessoResponse Gerar(Administrador administrador);
}
