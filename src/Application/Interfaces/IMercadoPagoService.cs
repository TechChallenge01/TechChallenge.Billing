namespace Application.Interfaces;

public interface IMercadoPagoService
{
    Task<CriarCobrancaResult> CriarCobranca(CriarCobrancaRequest request, CancellationToken ct);
    Task<bool> Estornar(string idTransacaoExterna, CancellationToken ct);
    Task<ConsultarPagamentoResult> ConsultarPagamento(string idTransacaoExterna, CancellationToken ct);
}

public class ConsultarPagamentoResult
{
    public string Status { get; set; } = string.Empty;  // "approved", "rejected", "pending", etc.
    public string? StatusDetail { get; set; }
}

public class CriarCobrancaRequest
{
    public Guid PagamentoId { get; set; }
    public Guid ClienteId { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
}

public class CriarCobrancaResult
{
    public string IdTransacao { get; set; } = string.Empty;
    public string LinkPagamento { get; set; } = string.Empty;
}
