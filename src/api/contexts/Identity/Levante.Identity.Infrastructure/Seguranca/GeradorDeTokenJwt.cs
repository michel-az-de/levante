using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Levante.Identity.Infrastructure.Seguranca;

/// <summary>Gera o JWT (HS256) de acesso do admin a partir de <see cref="JwtOptions"/>.</summary>
internal sealed class GeradorDeTokenJwt(IOptions<JwtOptions> options) : IGeradorDeToken
{
    public TokenDeAcessoResponse Gerar(Administrador administrador)
    {
        ArgumentNullException.ThrowIfNull(administrador);

        var opcoes = options.Value;
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcoes.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(opcoes.ExpiraEmMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, administrador.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, administrador.Email.Valor),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: opcoes.Issuer,
            audience: opcoes.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenDeAcessoResponse(jwt, opcoes.ExpiraEmMinutos * 60);
    }
}
