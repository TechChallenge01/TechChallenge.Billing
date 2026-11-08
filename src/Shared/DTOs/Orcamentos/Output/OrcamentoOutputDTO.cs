namespace Shared.DTOs.Orcamentos.Output;

public class OrcamentoOutputDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public List<ItemOrcamentoOutputDTO> Itens { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataExpiracao { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class ItemOrcamentoOutputDTO
{
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
