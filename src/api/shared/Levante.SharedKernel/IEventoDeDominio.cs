namespace Levante.SharedKernel;

/// <summary>
/// Marcador de evento de dominio. Eventos sao fatos PT, sem sufixo
/// (ver docs/convencao-de-nomes.md). A traducao para Integration Event
/// (Outbox -> Hiram) acontece na Infrastructure (fora da Fatia 0).
/// </summary>
public interface IEventoDeDominio;
