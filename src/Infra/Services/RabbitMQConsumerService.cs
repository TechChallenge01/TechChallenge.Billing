using System.Text;
using System.Text.Json;
using Application.Controllers.Orcamentos;
using Application.Controllers.Pagamentos;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.DTOs.Orcamentos.Request;

namespace Infra.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string Exchange = "tech.saga";

    public RabbitMQConsumerService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await BindQueue("billing.diagnostico.concluido", "diagnostico.concluido", stoppingToken);
        await BindQueue("billing.execucao.finalizada", "execucao.finalizada", stoppingToken);
        await BindQueue("billing.execucao.falhou", "execucao.falhou", stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync("billing.diagnostico.concluido", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("billing.execucao.finalizada", autoAck: false, consumer, stoppingToken);
        await _channel.BasicConsumeAsync("billing.execucao.falhou", autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task BindQueue(string queue, string routingKey, CancellationToken ct)
    {
        await _channel!.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await _channel.QueueBindAsync(queue, Exchange, routingKey, cancellationToken: ct);

        var dlq = $"{queue}.dlq";
        await _channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var orcamentoDataSource = scope.ServiceProvider.GetRequiredService<IOrcamentoDataSource>();
            var pagamentoDataSource = scope.ServiceProvider.GetRequiredService<IPagamentoDataSource>();
            var systemUserId = Guid.Parse(_configuration["SystemUserId"] ?? Guid.Empty.ToString());

            switch (routingKey)
            {
                case "diagnostico.concluido":
                {
                    var payload = JsonSerializer.Deserialize<DiagnosticoConcluido>(body)!;
                    var controller = new OrcamentoController(orcamentoDataSource, pagamentoDataSource);
                    await controller.Gerar(new GerarOrcamentoRequestDTO
                    {
                        OrdemServicoId = payload.OrdemServicoId,
                        ClienteId = payload.ClienteId,
                        Itens = payload.Itens.Select(i => new ItemOrcamentoRequestDTO
                        {
                            Descricao = i.Descricao,
                            Quantidade = i.Quantidade,
                            ValorUnitario = i.ValorUnitario,
                            Tipo = i.Tipo
                        }).ToList()
                    }, systemUserId, eventPublisher, default);
                    break;
                }
                case "execucao.falhou":
                {
                    var payload = JsonSerializer.Deserialize<ExecucaoFalhou>(body)!;
                    if (payload.PagamentoId.HasValue)
                    {
                        var mercadoPagoService = scope.ServiceProvider.GetRequiredService<IMercadoPagoService>();
                        var controller = new PagamentoController(pagamentoDataSource);
                        await controller.Estornar(payload.PagamentoId.Value, systemUserId, mercadoPagoService, eventPublisher, default);
                    }
                    break;
                }
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private record DiagnosticoConcluido(Guid OrdemServicoId, Guid ClienteId, List<DiagnosticoItem> Itens);
    private record DiagnosticoItem(string Descricao, int Quantidade, decimal ValorUnitario, string Tipo);
    private record ExecucaoFalhou(Guid OrdemServicoId, Guid? PagamentoId);
}
