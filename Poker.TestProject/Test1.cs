using Poker.Dominio;


namespace Poker.TestProject;

[TestClass]
public sealed class ValidadorMaoPokerTests
{
    [TestMethod]
    [DataRow("5C 5P 6E 7P KO", "2P 3E 8E 8O DO", "Jogador 2")]
    [DataRow("5O 8P 9E JE AP", "2P 5P 7O 8E QC", "Jogador 1")]
    [DataRow("2O 9P AE AC AP", "3O 6O 7O DO QO", "Jogador 2")]
    [DataRow("4O 6E 9C QC QP", "3O 6O 7C QO QE", "Jogador 1")]
    [DataRow("2C 2O 4P 4O 4E", "3P 3O 3E 9E 9O", "Jogador 1")]
    [DataRow("3O 4C 5E 6P 7O", "2C 2E 7P 7O 7C", "Jogador 1")]
    [DataRow("9O 9C 9E 9P 2O", "2C 2E 7P 7O 7C", "Jogador 1")]
    [DataRow("5E 6E 7E 8E 9E", "9O 9C 9E 9P 2O", "Jogador 1")]
    [DataRow("DC JC QC KC AC", "5E 6E 7E 8E 9E", "Jogador 1")]
    [DataRow("4O 4C 8E 8P KO", "4E 4P 8O 8C QO", "Jogador 1")]
    [DataRow("2O 5C 8E JP AO", "2P 5E 8C JO AC", "Empate")]
    public void Deve_Determinar_Vencedor_Corretamente(string mao1, string mao2, string resultadoEsperado)
    {
        // Act
        string resultado = ValidadorMaoPoker.DeterminarVencedor(mao1, mao2);

        // Assert
        Assert.Contains(resultado.Contains(resultadoEsperado), $"Esperado conter: '{resultadoEsperado}', mas obteve: '{resultado}'");
    }

    [TestMethod]
    [DataRow("5C 5P 6E 7P", "2P 3E 8E 8O DO", "Erro")] // Quantidade de cartas diferente de 5
    [DataRow("5C 5P 6E 7S KO", "2P 3E 8E 8O DO", "Erro")] // Naipe inexistente
    [DataRow("5C 5C 6E 7P KO", "2P 3E 8E 8O DO", "Erro")] // Carta repetida na mesma mão
    [DataRow("5C 5P 6E 7P KO", "5C 3E 8E 8O DO", "Erro")] // Carta compartilhada entre jogadores
    public void Deve_Retornar_Erro_Para_Maos_Invalidas(string mao1, string mao2, string erroEsperado)
    {
        // Act
        string resultado = ValidadorMaoPoker.DeterminarVencedor(mao1, mao2);

        // Assert
        StringAssert.StartsWith(resultado, erroEsperado);
    }
}