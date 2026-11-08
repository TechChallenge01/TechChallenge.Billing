using API.Extensions;
using Application.Controllers.Orcamentos;
using Application.Interfaces;

namespace API.EndPoints.Orcamentos;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orcamentos/{id:guid}", async (
            Guid id,
            IOrcamentoDataSource orcamentoDataSource,
            IPagamentoDataSource pagamentoDataSource,
            CancellationToken ct) =>
        {
            var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
            var result = await controller.GetById(id, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Orçamentos")
        .WithSummary("Busca um orçamento por ID");
    }
}
