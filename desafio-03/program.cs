using System.Globalization;

var cultura = new CultureInfo("pt-BR");
var hoje = DateOnly.FromDateTime(DateTime.Today);

Console.Write("Valor: ");

if (!decimal.TryParse(
        Console.ReadLine(),
        NumberStyles.Number,
        cultura,
        out decimal valor) ||
    valor <= 0)
{
    Console.WriteLine("Valor invalido");
    return;
}

Console.Write("Vencimento (dd/MM/aaaa): ");

if (!DateOnly.TryParseExact(
        Console.ReadLine(),
        "dd/MM/yyyy",
        cultura,
        DateTimeStyles.None,
        out DateOnly vencimento))
{
    Console.WriteLine("Data invalida");
    return;
}

int diasAtraso = Math.Max(
    0,
    hoje.DayNumber - vencimento.DayNumber
);

const decimal taxaDiaria = 0.025m;

decimal juros = valor * taxaDiaria * diasAtraso;
decimal total = valor + juros;

Console.WriteLine();
Console.WriteLine($"Dias de atraso: {diasAtraso}");
Console.WriteLine($"Juros: {juros.ToString("C2", cultura)}");
Console.WriteLine($"Total: {total.ToString("C2", cultura)}");