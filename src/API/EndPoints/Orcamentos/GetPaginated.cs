using API.Extensions;
using Application.Controllers.Orcamentos;
using Application.Interfaces;

namespace API.EndPoints.Orcamentos;

public class GetPaginated : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orcamentos", async (
            IOrcamentoDataSource orcamentoDataSource,
            IPagamentoDataSource pagamentoDataSource,
            CancellationToken ct,
            int page = 1, int pageSize = 50) =>
        {
            var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
            var result = await controller.GetPaginated(page, pageSize, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Orçamentos")
        .WithSummary("Lista orçamentos paginados");
    }
}
