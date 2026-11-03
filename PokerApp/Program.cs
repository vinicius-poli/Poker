using Poker.Dominio;

// Exemplo de teste rápido direto no console
string mao1 = "5C 5P 6E 7P KO";
string mao2 = "2P 3E 8E 8O DO";

string resultado = ValidadorMaoPoker.DeterminarVencedor(mao1, mao2);
Console.WriteLine($"Resultado: {resultado}");