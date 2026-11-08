using Application.Gateways.Orcamentos;
using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Domain.Aggregates.PagamentoAggregates;

namespace Application.UseCases.Orcamentos;

public class AprovarOrcamentoUseCase
{
    private readonly OrcamentoGateway _orcamentoGateway;
    private readonly PagamentoGateway _pagamentoGateway;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IEventPublisher _eventPublisher;

    private AprovarOrcamentoUseCase(
        OrcamentoGateway orcamentoGateway, PagamentoGateway pagamentoGateway,
        IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher)
    {
        _orcamentoGateway = orcamentoGateway;
        _pagamentoGateway = pagamentoGateway;
        _mercadoPagoService = mercadoPagoService;
        _eventPublisher = eventPublisher;
    }

    public static AprovarOrcamentoUseCase Create(
        OrcamentoGateway orcamentoGateway, PagamentoGateway pagamentoGateway,
        IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher)
        => new(orcamentoGateway, pagamentoGateway, mercadoPagoService, eventPublisher);

    public async Task<string> Run(Guid orcamentoId, Guid idUsuario, string emailCliente, CancellationToken ct)
    {
        var orcamento = await _orcamentoGateway.GetById(orcamentoId, ct)
            ?? throw new KeyNotFoundException($"Orçamento {orcamentoId} não encontrado.");

        orcamento.Aprovar(idUsuario);
        await _orcamentoGateway.Update(orcamento, ct);

        var pagamento = new Pagamento(orcamento.Id, orcamento.OrdemServicoId, orcamento.ClienteId, orcamento.ValorTotal, idUsuario);
        await _pagamentoGateway.Create(pagamento, ct);

        var cobranca = await _mercadoPagoService.CriarCobranca(new()
        {
            PagamentoId = pagamento.Id,
            ClienteId = orcamento.ClienteId,
            Valor = orcamento.ValorTotal,
            Descricao = $"OS {orcamento.OrdemServicoId} — Reparo veicular",
            EmailCliente = emailCliente
        }, ct);

        pagamento.RegistrarCobranca(cobranca.IdTransacao, cobranca.LinkPagamento, idUsuario);
        await _pagamentoGateway.Update(pagamento, ct);

        await _eventPublisher.PublishAsync("orcamento.aprovado", new
        {
            OrcamentoId = orcamento.Id,
            orcamento.OrdemServicoId,
            orcamento.ClienteId,
            PagamentoId = pagamento.Id,
            LinkPagamento = cobranca.LinkPagamento
        }, ct);

        return cobranca.LinkPagamento;
    }
}
