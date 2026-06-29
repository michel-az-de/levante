using Levante.Identity.Application.Autenticacao;
using Levante.Identity.Application.Ports;
using Levante.Identity.Domain.Administradores;
using Shouldly;
using Xunit;

namespace Levante.Identity.UnitTests;

public sealed class AutenticarCommandHandlerTests
{
    private const string EmailAdmin = "admin@levante.dev";

    private static Administrador AdminAtivo() => Administrador.Criar(new Email(EmailAdmin), "hash-correto");

    [Fact]
    public async Task Sucesso_retornaToken()
    {
        var handler = new AutenticarCommandHandler(new FakeRepo(AdminAtivo()), new FakeHash(verifica: true), new FakeToken());

        var resultado = await handler.Handle(new AutenticarCommand(EmailAdmin, "senha"), CancellationToken.None);

        resultado.Sucesso.ShouldBeTrue();
        resultado.Valor.ShouldNotBeNull();
        resultado.Valor.AccessToken.ShouldBe("token");
    }

    [Fact]
    public async Task SenhaErrada_falhaERegistraTentativa()
    {
        var admin = AdminAtivo();
        var repo = new FakeRepo(admin);
        var handler = new AutenticarCommandHandler(repo, new FakeHash(verifica: false), new FakeToken());

        var resultado = await handler.Handle(new AutenticarCommand(EmailAdmin, "errada"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("credenciais_invalidas");
        admin.TentativasDeLoginFalhas.ShouldBe(1);
        repo.Atualizacoes.ShouldBe(1);
    }

    [Fact]
    public async Task EmailDesconhecido_falhaSemEnumeracao()
    {
        var handler = new AutenticarCommandHandler(new FakeRepo(administrador: null), new FakeHash(verifica: true), new FakeToken());

        var resultado = await handler.Handle(new AutenticarCommand("nao@existe.com", "senha"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("credenciais_invalidas");
    }

    [Fact]
    public async Task Bloqueado_falha()
    {
        var admin = Administrador.Reconstituir(
            Guid.NewGuid(), new Email(EmailAdmin), "h", 5, DateTime.UtcNow.AddMinutes(10), true, DateTime.UtcNow);
        var handler = new AutenticarCommandHandler(new FakeRepo(admin), new FakeHash(verifica: true), new FakeToken());

        var resultado = await handler.Handle(new AutenticarCommand(EmailAdmin, "senha"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
        resultado.Erro.Codigo.ShouldBe("conta_bloqueada");
    }

    [Fact]
    public async Task EmailMalformado_falha()
    {
        var handler = new AutenticarCommandHandler(new FakeRepo(AdminAtivo()), new FakeHash(verifica: true), new FakeToken());

        var resultado = await handler.Handle(new AutenticarCommand("malformado", "senha"), CancellationToken.None);

        resultado.Falhou.ShouldBeTrue();
    }

    private sealed class FakeRepo(Administrador? administrador) : IAdministradorRepository
    {
        public int Atualizacoes { get; private set; }

        public Task<Administrador?> GetByEmailAsync(string email, CancellationToken ct) =>
            Task.FromResult(administrador is not null && administrador.Email.Valor == email ? administrador : null);

        public Task AddAsync(Administrador administrador, CancellationToken ct) => Task.CompletedTask;

        public Task UpdateAsync(Administrador administrador, CancellationToken ct)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task<bool> ExisteAlgumAsync(CancellationToken ct) => Task.FromResult(administrador is not null);
    }

    private sealed class FakeHash(bool verifica) : IHashDeSenha
    {
        public string Hash(string senha) => "hash:" + senha;

        public bool Verificar(string hash, string senha) => verifica;
    }

    private sealed class FakeToken : IGeradorDeToken
    {
        public TokenDeAcessoResponse Gerar(Administrador administrador) => new("token", 3600);
    }
}
