using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Extensions;
using Application.Controllers.Pagamentos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.EndPoints.Webhook;

public class MercadoPagoWebhook : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/mercadopago", async (
            HttpRequest request,
            [FromBody] JsonElement body,
            IPagamentoDataSource dataSource,
            IEventPublisher eventPublisher,
            IMercadoPagoService mercadoPagoService,
            IConfiguration configuration,
            CancellationToken ct) =>
        {
            // Valida a assinatura x-signature antes de processar
            if (!ValidarAssinatura(request, body, configuration))
                return Results.Unauthorized();

            var type = body.TryGetProperty("type", out var t) ? t.GetString() : null;
            if (type != "payment")
                return Results.Ok(); // ignora outros tópicos (contestações, etc.)

            var dataId = body.GetProperty("data").GetProperty("id").GetString() ?? string.Empty;
            if (string.IsNullOrEmpty(dataId))
                return Results.BadRequest("data.id ausente.");

            // Consulta o status real do pagamento via API do MP
            var statusResult = await mercadoPagoService.ConsultarPagamento(dataId, ct);
            var systemUserId = Guid.Parse(configuration["SystemUserId"] ?? Guid.Empty.ToString());
            var controller = new PagamentoController(dataSource);

            var result = statusResult.Status switch
            {
                "approved" => await controller.ConfirmarPorWebhook(dataId, systemUserId, eventPublisher, ct),
                "rejected" or "cancelled" or "canceled" => await controller.FalharPorWebhook(
                    dataId, $"Pagamento {statusResult.Status}: {statusResult.StatusDetail}", systemUserId, eventPublisher, ct),
                // pending / in_process / in_review — aguardar próxima notificação
                _ => null
            };

            return result is null ? Results.Ok() : result.ToMinimalApiResult();
        })
        .WithTags("Webhook")
        .WithSummary("Recebe notificações de pagamento do Mercado Pago");
    }

    private static bool ValidarAssinatura(HttpRequest request, JsonElement body, IConfiguration configuration)
    {
        var secret = configuration["MercadoPago:WebhookSecret"];
        if (string.IsNullOrEmpty(secret))
            return true; // sem segredo configurado → aceita (modo dev)

        if (!request.Headers.TryGetValue("x-signature", out var xSignature) ||
            !request.Headers.TryGetValue("x-request-id", out var xRequestId))
            return false;

        // Formato: ts=<timestamp>,v1=<hash>
        var parts = xSignature.ToString().Split(',');
        var ts = parts.FirstOrDefault(p => p.StartsWith("ts="))?.Split('=')[1];
        var v1 = parts.FirstOrDefault(p => p.StartsWith("v1="))?.Split('=')[1];

        if (ts is null || v1 is null) return false;

        var dataId = body.TryGetProperty("data", out var data) &&
                     data.TryGetProperty("id", out var id) ? id.GetString() : string.Empty;

        // String a assinar: id:<data.id>;request-id:<x-request-id>;ts:<ts>
        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts}";
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(manifest));
        var computed = Convert.ToHexString(hash).ToLower();

        return computed == v1;
    }
}
