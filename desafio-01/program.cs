using System.Text.Json;
using System.Text.Json.Serialization;

try
{
    const string caminhoArquivo = "vendas.json";

    string json = File.ReadAllText(caminhoArquivo);

    var dados = JsonSerializer.Deserialize<DadosVendas>(
        json,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

    if (dados is null || dados.Vendas.Count == 0)
    {
        Console.WriteLine("Nenhuma venda foi encontrada.");
        return;
    }

    var comissoesPorVendedor = dados.Vendas
        .GroupBy(venda => venda.Vendedor)
        .Select(grupo => new
        {
            Vendedor = grupo.Key,
            TotalVendido = grupo.Sum(venda => venda.Valor),
            Comissao = grupo.Sum(venda => CalcularComissao(venda.Valor))
        })
        .OrderBy(resultado => resultado.Vendedor);

    Console.WriteLine("Comissões por vendedor:");
    Console.WriteLine();

    foreach (var resultado in comissoesPorVendedor)
    {
        Console.WriteLine($"Vendedor: {resultado.Vendedor}");
        Console.WriteLine($"Total vendido: {resultado.TotalVendido:C}");
        Console.WriteLine($"Comissão: {resultado.Comissao:C}");
        Console.WriteLine();
    }
}
catch (FileNotFoundException)
{
    Console.WriteLine("O arquivo vendas.json não encontrado.");
}
catch (JsonException)
{
    Console.WriteLine("arquivo JSON inválido.");
}
catch (IOException ex)
{
    Console.WriteLine($"Nao foi possível ler o arquivo: {ex.Message}");
}

static decimal CalcularComissao(decimal valorVenda)
{
    if (valorVenda < 100m)
        return 0m;

    if (valorVenda < 500m)
        return valorVenda * 0.01m;

    return valorVenda * 0.05m;
}

public sealed class DadosVendas
{
    [JsonPropertyName("vendas")]
    public List<Venda> Vendas { get; init; } = [];
}

public sealed class Venda
{
    [JsonPropertyName("vendedor")]
    public string Vendedor { get; init; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; init; }
}