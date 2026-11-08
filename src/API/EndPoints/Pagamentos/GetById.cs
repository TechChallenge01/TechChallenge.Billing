using API.Extensions;
using Application.Controllers.Pagamentos;
using Application.Interfaces;

namespace API.EndPoints.Pagamentos;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/pagamentos/{id:guid}", async (
            Guid id,
            IPagamentoDataSource dataSource,
            CancellationToken ct) =>
        {
            var controller = new PagamentoController(dataSource);
            var result = await controller.GetById(id, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Pagamentos")
        .WithSummary("Busca um pagamento por ID");
    }
}
