using System.Net;
using Domain.Aggregates.PagamentoAggregates;
using Shared.DTOs.Pagamentos.Output;
using Shared.Result;

namespace Application.Presenters.Pagamentos;

public class PagamentoPresenter
{
    private readonly string _message;

    public PagamentoPresenter(string message) => _message = message;

    public ICommandResult<PagamentoOutputDTO> TransformObject(Pagamento pagamento)
        => new CommandResult<PagamentoOutputDTO>(_message, HttpStatusCode.OK, Transform(pagamento));

    public ICommandResult<T> Created<T>(T data)
        => new CommandResult<T>(_message, HttpStatusCode.Created, data);

    public ICommandResult NoContent()
        => new CommandResult(_message, HttpStatusCode.NoContent);

    public ICommandResult NotFound(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.NotFound);

    public ICommandResult BadRequest(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.BadRequest);

    public ICommandResult InternalError(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.InternalServerError);

    private static PagamentoOutputDTO Transform(Pagamento p) => new()
    {
        Id = p.Id,
        OrcamentoId = p.OrcamentoId,
        OrdemServicoId = p.OrdemServicoId,
        Valor = p.Valor,
        Status = p.Status.ToString(),
        LinkPagamento = p.LinkPagamento,
        MotivoFalha = p.MotivoFalha,
        DataCriacao = p.DataCriacao,
        DataAtualizacao = p.DataAtualizacao
    };
}
