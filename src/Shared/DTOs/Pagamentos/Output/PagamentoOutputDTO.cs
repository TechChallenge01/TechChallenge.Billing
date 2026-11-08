namespace Shared.DTOs.Pagamentos.Output;

public class PagamentoOutputDTO
{
    public Guid Id { get; set; }
    public Guid OrcamentoId { get; set; }
    public Guid OrdemServicoId { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LinkPagamento { get; set; }
    public string? MotivoFalha { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
