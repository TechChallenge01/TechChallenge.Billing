using Domain.BaseEntity;
using Domain.Enums;

namespace Domain.Aggregates.PagamentoAggregates;

public class Pagamento : Base
{
    public Pagamento(Guid orcamentoId, Guid ordemServicoId, Guid clienteId, decimal valor, Guid idUsuarioCriacao)
        : base(idUsuarioCriacao, DateTime.UtcNow)
    {
        Id = Guid.NewGuid();
        OrcamentoId = orcamentoId;
        OrdemServicoId = ordemServicoId;
        ClienteId = clienteId;
        Valor = valor;
        Status = EStatusPagamento.Pendente;
    }

    // Construtor de reconstituição
    public Pagamento(
        Guid id, Guid orcamentoId, Guid ordemServicoId, Guid clienteId,
        decimal valor, EStatusPagamento status, string? idTransacaoExterna,
        string? linkPagamento, string? motivoFalha,
        Guid idUsuarioCriacao, DateTime dataCriacao,
        Guid? idUsuarioAtualizacao, DateTime? dataAtualizacao, bool ativo)
        : base(idUsuarioCriacao, dataCriacao, idUsuarioAtualizacao, dataAtualizacao)
    {
        Id = id;
        OrcamentoId = orcamentoId;
        OrdemServicoId = ordemServicoId;
        ClienteId = clienteId;
        Valor = valor;
        Status = status;
        IdTransacaoExterna = idTransacaoExterna;
        LinkPagamento = linkPagamento;
        MotivoFalha = motivoFalha;
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public Guid OrcamentoId { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public decimal Valor { get; private set; }
    public EStatusPagamento Status { get; private set; }
    public string? IdTransacaoExterna { get; private set; }
    public string? LinkPagamento { get; private set; }
    public string? MotivoFalha { get; private set; }

    public void RegistrarCobranca(string idTransacaoExterna, string linkPagamento, Guid idUsuario)
    {
        if (Status != EStatusPagamento.Pendente)
            throw new InvalidOperationException($"Não é possível registrar cobrança para pagamento com status {Status}.");

        IdTransacaoExterna = idTransacaoExterna;
        LinkPagamento = linkPagamento;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }

    public void Confirmar(Guid idUsuario)
    {
        if (Status != EStatusPagamento.Pendente)
            throw new InvalidOperationException($"Não é possível confirmar pagamento com status {Status}.");

        Status = EStatusPagamento.Aprovado;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }

    public void Recusar(string motivo, Guid idUsuario)
    {
        if (Status != EStatusPagamento.Pendente)
            throw new InvalidOperationException($"Não é possível recusar pagamento com status {Status}.");

        Status = EStatusPagamento.Recusado;
        MotivoFalha = motivo;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }

    public void Estornar(Guid idUsuario)
    {
        if (Status != EStatusPagamento.Aprovado)
            throw new InvalidOperationException("Somente pagamentos aprovados podem ser estornados.");

        Status = EStatusPagamento.Estornado;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
    }
}
