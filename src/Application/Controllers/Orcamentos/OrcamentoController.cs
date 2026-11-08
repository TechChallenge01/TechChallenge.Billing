using Application.Gateways.Orcamentos;
using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.Presenters.Orcamentos;
using Application.UseCases.Orcamentos;
using Shared.DTOs.Orcamentos.Request;
using Shared.Result;

namespace Application.Controllers.Orcamentos;

public class OrcamentoController
{
    private readonly IOrcamentoDataSource _orcamentoDataSource;
    private readonly IPagamentoDataSource _pagamentoDataSource;

    public OrcamentoController(IOrcamentoDataSource orcamentoDataSource, IPagamentoDataSource pagamentoDataSource)
    {
        _orcamentoDataSource = orcamentoDataSource;
        _pagamentoDataSource = pagamentoDataSource;
    }

    public async Task<ICommandResult> Gerar(
        GerarOrcamentoRequestDTO request, Guid idUsuario,
        IEventPublisher eventPublisher, CancellationToken ct, IMetricsService? metrics = null)
    {
        var presenter = new OrcamentoPresenter("Orçamento gerado com sucesso.");
        try
        {
            var gateway = OrcamentoGateway.Create(_orcamentoDataSource);
            var useCase = GerarOrcamentoUseCase.Create(gateway, eventPublisher);
            var id = await useCase.Run(request, idUsuario, ct);
            metrics?.Increment("orcamento.gerado", new[] { "servico:billing" });
            return presenter.Created(id);
        }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> Aprovar(
        Guid orcamentoId, string emailCliente, Guid idUsuario,
        IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher,
        CancellationToken ct, IMetricsService? metrics = null)
    {
        var presenter = new OrcamentoPresenter("Orçamento aprovado e link de pagamento gerado.");
        try
        {
            var orcamentoGateway = OrcamentoGateway.Create(_orcamentoDataSource);
            var pagamentoGateway = PagamentoGateway.Create(_pagamentoDataSource);
            var useCase = AprovarOrcamentoUseCase.Create(orcamentoGateway, pagamentoGateway, mercadoPagoService, eventPublisher);
            var link = await useCase.Run(orcamentoId, idUsuario, emailCliente, ct);
            metrics?.Increment("orcamento.aprovado", new[] { "servico:billing" });
            return presenter.Created(link);
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> Rejeitar(
        Guid orcamentoId, RejeitarOrcamentoRequestDTO request, Guid idUsuario,
        IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new OrcamentoPresenter("Orçamento rejeitado.");
        try
        {
            var gateway = OrcamentoGateway.Create(_orcamentoDataSource);
            var useCase = RejeitarOrcamentoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(orcamentoId, request.Motivo, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> GetById(Guid id, CancellationToken ct)
    {
        var presenter = new OrcamentoPresenter("Orçamento encontrado.");
        try
        {
            var gateway = OrcamentoGateway.Create(_orcamentoDataSource);
            var useCase = GetByIdUseCase.Create(gateway);
            var orcamento = await useCase.Run(id, ct);
            if (orcamento is null) return presenter.NotFound("Orçamento não encontrado.");
            return presenter.TransformObject(orcamento);
        }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var presenter = new OrcamentoPresenter("Lista de orçamentos.");
        try
        {
            var gateway = OrcamentoGateway.Create(_orcamentoDataSource);
            var useCase = GetPaginatedUseCase.Create(gateway);
            var (items, total) = await useCase.Run(page, pageSize, ct);
            return presenter.TransformPaged(items, page, total, pageSize);
        }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }
}
