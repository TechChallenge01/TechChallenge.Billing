using API.Extensions;
using Application.Controllers.Orcamentos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Orcamentos.Request;

namespace API.EndPoints.Orcamentos;

public class RejeitarOrcamento : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/orcamentos/{id:guid}/rejeitar", async (
            Guid id,
            [FromBody] RejeitarOrcamentoRequestDTO request,
            HttpContext ctx,
            IOrcamentoDataSource orcamentoDataSource,
            IPagamentoDataSource pagamentoDataSource,
            IEventPublisher eventPublisher,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
            var result = await controller.Rejeitar(id, request, idUsuario, eventPublisher, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Orçamentos")
        .WithSummary("Rejeita um orçamento");
    }
}
