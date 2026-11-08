using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;

namespace Infra.Services;

public class MercadoPagoService : IMercadoPagoService
{
    public MercadoPagoService(IConfiguration configuration)
    {
        MercadoPagoConfig.AccessToken = configuration["MercadoPago:AccessToken"]
            ?? throw new InvalidOperationException("MercadoPago:AccessToken não configurado.");
    }

    public async Task<CriarCobrancaResult> CriarCobranca(CriarCobrancaRequest request, CancellationToken ct)
    {
        var client = new PaymentClient();
        var payment = await client.CreateAsync(new PaymentCreateRequest
        {
            TransactionAmount = request.Valor,
            Description = request.Descricao,
            ExternalReference = request.PagamentoId.ToString(),
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest
            {
                Email = request.EmailCliente
            }
        }, cancellationToken: ct);

        return new CriarCobrancaResult
        {
            IdTransacao = payment.Id?.ToString() ?? throw new InvalidOperationException("MercadoPago não retornou ID da transação."),
            LinkPagamento = payment.PointOfInteraction?.TransactionData?.TicketUrl
                ?? throw new InvalidOperationException("MercadoPago não retornou link de pagamento.")
        };
    }

    public async Task<bool> Estornar(string idTransacaoExterna, CancellationToken ct)
    {
        var client = new PaymentClient();
        var refund = await client.RefundAsync(long.Parse(idTransacaoExterna), cancellationToken: ct);
        return refund is not null;
    }

    public async Task<ConsultarPagamentoResult> ConsultarPagamento(string idTransacaoExterna, CancellationToken ct)
    {
        var client = new PaymentClient();
        var payment = await client.GetAsync(long.Parse(idTransacaoExterna), cancellationToken: ct);
        return new ConsultarPagamentoResult
        {
            Status = payment.Status ?? "unknown",
            StatusDetail = payment.StatusDetail
        };
    }
}
