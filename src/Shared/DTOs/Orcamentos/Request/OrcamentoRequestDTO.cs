namespace Shared.DTOs.Orcamentos.Request;

public class GerarOrcamentoRequestDTO
{
    public Guid OrdemServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public List<ItemOrcamentoRequestDTO> Itens { get; set; } = new();
}

public class ItemOrcamentoRequestDTO
{
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public string Tipo { get; set; } = string.Empty;
}

public class RejeitarOrcamentoRequestDTO
{
    public string Motivo { get; set; } = string.Empty;
}
