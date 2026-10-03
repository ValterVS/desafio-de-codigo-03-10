using System.Text.Json;
using System.Text.Json.Serialization;

const string caminhoEstoque = "estoque.json";
const string caminhoMovimentacoes = "movimentacoes.json";

var opcoesJson = new JsonSerializerOptions
{
    WriteIndented = true
};

opcoesJson.Converters.Add(new JsonStringEnumConverter());

try
{
    var dadosEstoque = CarregarEstoque();
    var movimentacoes = CarregarMovimentacoes();

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("=== MOVIMENTACAO DE ESTOQUE ===");
        Console.WriteLine("Digite 0 como codigo do produto para encerrar.");
        Console.Write("Codigo do produto: ");

        if (!int.TryParse(Console.ReadLine(), out int codigoProduto))
        {
            Console.WriteLine("Codigo invalido.");
            continue;
        }

        if (codigoProduto == 0)
            break;

        var produto = dadosEstoque.Produtos
            .SingleOrDefault(p => p.CodigoProduto == codigoProduto);

        if (produto is null)
        {
            Console.WriteLine("Produto nao encontrado.");
            continue;
        }

        Console.WriteLine($"Produto: {produto.DescricaoProduto}");
        Console.WriteLine($"Estoque atual: {produto.QuantidadeEstoque}");
        Console.Write("Movimentacao\nE - Entrada\nS - saida\nDigite a opcao: ");

        string opcao = Console.ReadLine()?.Trim().ToUpperInvariant() ?? "";

        TipoMovimentacao tipoMovimentacao;

        if (opcao == "E")
        {
            tipoMovimentacao = TipoMovimentacao.Entrada;
        }
        else if (opcao == "S")
        {
            tipoMovimentacao = TipoMovimentacao.Saida;
        }
        else
        {
            Console.WriteLine("Tipo de movimentação invalido.");
            continue;
        }

        Console.Write("Quantidade: ");

        if (!int.TryParse(Console.ReadLine(), out int quantidade) ||
            quantidade <= 0)
        {
            Console.WriteLine("A quantidade deve ser maior que zero.");
            continue;
        }

        if (tipoMovimentacao == TipoMovimentacao.Saida &&
            quantidade > produto.QuantidadeEstoque)
        {
            Console.WriteLine(
                $"Saida nao permitida. Estoque disponivel: " +
                $"{produto.QuantidadeEstoque}."
            );

            continue;
        }

        if (tipoMovimentacao == TipoMovimentacao.Entrada)
            produto.QuantidadeEstoque += quantidade;
        else
            produto.QuantidadeEstoque -= quantidade;

        var movimentacao = new Movimentacao
        {
            Id = Guid.NewGuid(),
            CodigoProduto = produto.CodigoProduto,
            DescricaoProduto = produto.DescricaoProduto,
            Tipo = tipoMovimentacao,
            Descricao = tipoMovimentacao == TipoMovimentacao.Entrada
                ? "Entrada de mercadoria"
                : "Saida de mercadoria",
            Quantidade = quantidade,
            DataHora = DateTimeOffset.Now,
            EstoqueFinal = produto.QuantidadeEstoque
        };

        movimentacoes.Add(movimentacao);

        SalvarEstoque(dadosEstoque);
        SalvarMovimentacoes(movimentacoes);

        Console.WriteLine();
        Console.WriteLine("Movimentacao realizada com sucesso.");
        Console.WriteLine($"Identificador: {movimentacao.Id}");
        Console.WriteLine($"Descricao: {movimentacao.Descricao}");
        Console.WriteLine(
            $"Estoque final de {produto.DescricaoProduto}: " +
            $"{produto.QuantidadeEstoque}"
        );
    }
}
catch (FileNotFoundException)
{
    Console.WriteLine("Arquivo estoque.json nao encontrado.");
}
catch (JsonException)
{
    Console.WriteLine("Um dos arquivos contem um JSON inválido.");
}
catch (IOException ex)
{
    Console.WriteLine($"Erro ao acessar os arquivos: {ex.Message}");
}

ArquivoEstoque CarregarEstoque()
{
    string json = File.ReadAllText(caminhoEstoque);

    return JsonSerializer.Deserialize<ArquivoEstoque>(json, opcoesJson)
           ?? throw new JsonException("Não foi possível ler o estoque.");
}

List<Movimentacao> CarregarMovimentacoes()
{
    if (!File.Exists(caminhoMovimentacoes))
        return [];

    string json = File.ReadAllText(caminhoMovimentacoes);

    return JsonSerializer.Deserialize<List<Movimentacao>>(json, opcoesJson)
           ?? [];
}

void SalvarEstoque(ArquivoEstoque estoque)
{
    string json = JsonSerializer.Serialize(estoque, opcoesJson);
    File.WriteAllText(caminhoEstoque, json);
}

void SalvarMovimentacoes(List<Movimentacao> movimentacoes)
{
    string json = JsonSerializer.Serialize(movimentacoes, opcoesJson);
    File.WriteAllText(caminhoMovimentacoes, json);
}

public enum TipoMovimentacao
{
    Entrada,
    Saida
}

public sealed class ArquivoEstoque
{
    [JsonPropertyName("estoque")]
    public List<Produto> Produtos { get; init; } = [];
}

public sealed class Produto
{
    [JsonPropertyName("codigoProduto")]
    public int CodigoProduto { get; init; }

    [JsonPropertyName("descricaoProduto")]
    public string DescricaoProduto { get; init; } = string.Empty;

    [JsonPropertyName("estoque")]
    public int QuantidadeEstoque { get; set; }
}

public sealed class Movimentacao
{
    public Guid Id { get; init; }
    public int CodigoProduto { get; init; }
    public string DescricaoProduto { get; init; } = string.Empty;


    public TipoMovimentacao Tipo { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public int Quantidade { get; init; }


    public DateTimeOffset DataHora { get; init; }
    public int EstoqueFinal { get; init; }
}