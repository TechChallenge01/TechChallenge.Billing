namespace Shared.DTOs.Orcamentos.Input;

public class OrcamentoInputDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public List<ItemOrcamentoInputDTO> Itens { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataExpiracao { get; set; }
    public string? MotivoRejeicao { get; set; }
    public Guid IdUsuarioCriacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public Guid? IdUsuarioAtualizacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
}

public class ItemOrcamentoInputDTO
{
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
