namespace Levante.Identity.Domain.Administradores;

/// <summary>
/// Agregado do administrador (admin unico). Guarda apenas o HASH da senha e o
/// estado de bloqueio. Hashing e geracao de token vivem fora do dominio (ports).
/// </summary>
public sealed class Administrador
{
    public const int MaxTentativasDeLoginFalhas = 5;
    public const int MinutosDeBloqueio = 15;

    private Administrador(
        Guid id,
        Email email,
        string senhaHash,
        int tentativasDeLoginFalhas,
        DateTime? bloqueadoAte,
        bool ativo,
        DateTime dataCriacao)
    {
        Id = id;
        Email = email;
        SenhaHash = senhaHash;
        TentativasDeLoginFalhas = tentativasDeLoginFalhas;
        BloqueadoAte = bloqueadoAte;
        Ativo = ativo;
        DataCriacao = dataCriacao;
    }

    public Guid Id { get; }

    public Email Email { get; }

    public string SenhaHash { get; private set; }

    public int TentativasDeLoginFalhas { get; private set; }

    public DateTime? BloqueadoAte { get; private set; }

    public bool Ativo { get; private set; }

    public DateTime DataCriacao { get; }

    public static Administrador Criar(Email email, string senhaHash)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaHash);

        return new Administrador(
            Guid.NewGuid(), email, senhaHash, tentativasDeLoginFalhas: 0,
            bloqueadoAte: null, ativo: true, DateTime.UtcNow);
    }

    public static Administrador Reconstituir(
        Guid id,
        Email email,
        string senhaHash,
        int tentativasDeLoginFalhas,
        DateTime? bloqueadoAte,
        bool ativo,
        DateTime dataCriacao) =>
        new(id, email, senhaHash, tentativasDeLoginFalhas, bloqueadoAte, ativo, dataCriacao);

    public bool EstaBloqueado(DateTime agora) => BloqueadoAte.HasValue && BloqueadoAte.Value > agora;

    /// <summary>Registra uma tentativa de login falha; bloqueia apos o limite.</summary>
    public void RegistrarFalhaDeLogin()
    {
        TentativasDeLoginFalhas++;
        if (TentativasDeLoginFalhas >= MaxTentativasDeLoginFalhas)
        {
            BloqueadoAte = DateTime.UtcNow.AddMinutes(MinutosDeBloqueio);
        }
    }

    /// <summary>Limpa o contador de falhas e o bloqueio (login bem-sucedido).</summary>
    public void ResetarFalhas()
    {
        TentativasDeLoginFalhas = 0;
        BloqueadoAte = null;
    }

    public void DefinirSenhaHash(string senhaHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaHash);
        SenhaHash = senhaHash;
    }
}
