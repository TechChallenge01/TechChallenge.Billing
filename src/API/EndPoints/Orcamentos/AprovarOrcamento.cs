using API.Extensions;
using Application.Controllers.Orcamentos;
using Application.Interfaces;

namespace API.EndPoints.Orcamentos;

public class AprovarOrcamento : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orcamentos/{id:guid}/aprovar", async (
            Guid id,
            HttpContext ctx,
            IOrcamentoDataSource orcamentoDataSource,
            IPagamentoDataSource pagamentoDataSource,
            IMercadoPagoService mercadoPagoService,
            IEventPublisher eventPublisher,
            IMetricsService metrics,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var email = ctx.User.GetEmail();
            var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
            var result = await controller.Aprovar(id, email, idUsuario, mercadoPagoService, eventPublisher, ct, metrics);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Orçamentos")
        .WithSummary("Aprova um orçamento e gera o link de pagamento");
    }
}
