using Application.Gateways.Pagamentos;
using Application.Interfaces;
using Application.Presenters.Pagamentos;
using Application.UseCases.Pagamentos;
using Shared.Result;

namespace Application.Controllers.Pagamentos;

public class PagamentoController
{
    private readonly IPagamentoDataSource _dataSource;

    public PagamentoController(IPagamentoDataSource dataSource) => _dataSource = dataSource;

    public async Task<ICommandResult> GetById(Guid id, CancellationToken ct)
    {
        var presenter = new PagamentoPresenter("Pagamento encontrado.");
        try
        {
            var gateway = PagamentoGateway.Create(_dataSource);
            var useCase = GetByIdUseCase.Create(gateway);
            var pagamento = await useCase.Run(id, ct);
            if (pagamento is null) return presenter.NotFound("Pagamento não encontrado.");
            return presenter.TransformObject(pagamento);
        }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> ConfirmarPorWebhook(
        string idTransacaoExterna, Guid idUsuarioSistema,
        IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new PagamentoPresenter("Pagamento confirmado.");
        try
        {
            var gateway = PagamentoGateway.Create(_dataSource);
            var useCase = ConfirmarPagamentoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(idTransacaoExterna, idUsuarioSistema, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> FalharPorWebhook(
        string idTransacaoExterna, string motivo, Guid idUsuarioSistema,
        IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new PagamentoPresenter("Pagamento marcado como falho.");
        try
        {
            var gateway = PagamentoGateway.Create(_dataSource);
            var useCase = FalharPagamentoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(idTransacaoExterna, motivo, idUsuarioSistema, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> Estornar(
        Guid pagamentoId, Guid idUsuario,
        IMercadoPagoService mercadoPagoService, IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new PagamentoPresenter("Pagamento estornado.");
        try
        {
            var gateway = PagamentoGateway.Create(_dataSource);
            var useCase = EstornarPagamentoUseCase.Create(gateway, mercadoPagoService, eventPublisher);
            await useCase.Run(pagamentoId, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }
}
