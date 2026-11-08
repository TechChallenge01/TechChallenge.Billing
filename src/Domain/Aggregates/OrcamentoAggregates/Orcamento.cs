using Domain.BaseEntity;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates.OrcamentoAggregates;

public class Orcamento : Base
{
    private readonly List<ItemOrcamento> _itens = new();

    public Orcamento(Guid ordemServicoId, Guid clienteId, List<ItemOrcamento> itens, Guid idUsuarioCriacao)
        : base(idUsuarioCriacao, DateTime.UtcNow)
    {
        Id = Guid.NewGuid();
        OrdemServicoId = ordemServicoId;
        ClienteId = clienteId;
        _itens = itens;
        Status = EStatusOrcamento.Pendente;
        ValorTotal = _itens.Sum(i => i.ValorTotal);
        DataExpiracao = DateTime.UtcNow.AddDays(3);
    }

    // Construtor de reconstituição
    public Orcamento(
        Guid id, Guid ordemServicoId, Guid clienteId, List<ItemOrcamento> itens,
        EStatusOrcamento status, decimal valorTotal, DateTime dataExpiracao,
        string? motivoRejeicao, Guid idUsuarioCriacao, DateTime dataCriacao,
        Guid? idUsuarioAtualizacao, DateTime? dataAtualizacao, bool ativo)
        : base(idUsuarioCriacao, dataCriacao, idUsuarioAtualizacao, dataAtualizacao)
    {
        Id = id;
        OrdemServicoId = ordemServicoId;
        ClienteId = clienteId;
        _itens = itens;
        Status = status;
        ValorTotal = valorTotal;
        DataExpiracao = dataExpiracao;
        MotivoRejeicao = motivoRejeicao;
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public IReadOnlyList<ItemOrcamento> Itens => _itens.AsReadOnly();
    public EStatusOrcamento Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime DataExpiracao { get; private set; }
    public string? MotivoRejeicao { get; private set; }

    public void Aprovar(Guid idUsuario)
    {
        if (Status != EStatusOrcamento.Pendente)
            throw new InvalidOperationException($"Não é possível aprovar um orçamento com status {Status}.");
        if (DateTime.UtcNow > DataExpiracao)
            throw new InvalidOperationException("Orçamento expirado. Solicite um novo orçamento.");

        Status = EStatusOrcamento.Aprovado;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }

    public void Rejeitar(string motivo, Guid idUsuario)
    {
        if (Status != EStatusOrcamento.Pendente)
            throw new InvalidOperationException($"Não é possível rejeitar um orçamento com status {Status}.");

        Status = EStatusOrcamento.Rejeitado;
        MotivoRejeicao = motivo;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }

    public void Expirar(Guid idUsuario)
    {
        if (Status != EStatusOrcamento.Pendente)
            return;

        Status = EStatusOrcamento.Expirado;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }
}
