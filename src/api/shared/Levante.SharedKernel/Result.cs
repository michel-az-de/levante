namespace Levante.SharedKernel;

/// <summary>
/// Classe do erro de negocio. E o que a borda HTTP usa para escolher o status
/// (400/404/409/500) sem conhecer os codigos de cada contexto.
/// </summary>
public enum TipoErro
{
    /// <summary>Falha nao classificada (vira 500 na borda; classifique na Application).</summary>
    Falha = 0,
    Validacao,
    NaoEncontrado,
    Conflito,
}

/// <summary>
/// Erro de negocio (codigo + mensagem em PT, voltada ao usuario). Prefira as
/// factories tipadas: o Tipo dirige o mapeamento HTTP na borda.
/// </summary>
public sealed record Error(string Codigo, string Mensagem, TipoErro Tipo = TipoErro.Falha)
{
    public static readonly Error Nenhum = new(string.Empty, string.Empty);

    public static Error Validacao(string codigo, string mensagem) =>
        new(codigo, mensagem, TipoErro.Validacao);

    public static Error NaoEncontrado(string codigo, string mensagem) =>
        new(codigo, mensagem, TipoErro.NaoEncontrado);

    public static Error Conflito(string codigo, string mensagem) =>
        new(codigo, mensagem, TipoErro.Conflito);
}

/// <summary>
/// Result pattern. Fluxo de negocio esperado nao usa exception (ver CLAUDE.md).
/// </summary>
public class Result
{
    protected Result(bool sucesso, Error erro)
    {
        ArgumentNullException.ThrowIfNull(erro);
        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }

    public bool Falhou => !Sucesso;

    public Error Erro { get; }

    public static Result Ok() => new(sucesso: true, Error.Nenhum);

    public static Result<T> Ok<T>(T valor) => new(valor, sucesso: true, Error.Nenhum);

    public static Result Falha(Error erro) => new(sucesso: false, erro);

    public static Result<T> Falha<T>(Error erro) => new(valor: default, sucesso: false, erro);
}

/// <summary>
/// Result com valor. <see cref="Valor"/> e preenchido somente quando <see cref="Result.Sucesso"/>.
/// </summary>
public sealed class Result<T> : Result
{
    internal Result(T? valor, bool sucesso, Error erro)
        : base(sucesso, erro) => Valor = valor;

    public T? Valor { get; }
}
