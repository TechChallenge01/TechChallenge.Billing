using API.Extensions;
using Application.Controllers.Orcamentos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Orcamentos.Request;

namespace API.EndPoints.Orcamentos;

public class GerarOrcamento : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orcamentos", async (
            [FromBody] GerarOrcamentoRequestDTO request,
            HttpContext ctx,
            IOrcamentoDataSource orcamentoDataSource,
            IPagamentoDataSource pagamentoDataSource,
            IEventPublisher eventPublisher,
            IMetricsService metrics,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
            var result = await controller.Gerar(request, idUsuario, eventPublisher, ct, metrics);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Orçamentos")
        .WithSummary("Gera um novo orçamento para uma OS");
    }
}
