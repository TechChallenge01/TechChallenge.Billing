namespace Domain.ValueObjects;

public class ItemOrcamento
{
    public ItemOrcamento(string descricao, int quantidade, decimal valorUnitario, string tipo)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        if (valorUnitario < 0)
            throw new ArgumentException("Valor unitário não pode ser negativo.", nameof(valorUnitario));

        Descricao = descricao;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        Tipo = tipo;
    }

    public string Descricao { get; }
    public int Quantidade { get; }
    public decimal ValorUnitario { get; }
    public string Tipo { get; }
    public decimal ValorTotal => Quantidade * ValorUnitario;
}
