namespace Shared.DTOs.Pagamentos.Input;

public class PagamentoInputDTO
{
    public Guid Id { get; set; }
    public Guid OrcamentoId { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public decimal Valor { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? IdTransacaoExterna { get; set; }
    public string? LinkPagamento { get; set; }
    public string? MotivoFalha { get; set; }
    public Guid IdUsuarioCriacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public Guid? IdUsuarioAtualizacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
}
