using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Dominio
{
    // --- ENUMS ---
    public enum Naipe
    {
        Ouro = 'O',
        Copas = 'C',
        Espadas = 'E',
        Paus = 'P'
    }

    public enum RankingMao
    {
        CartaAlta = 1,
        UmPar = 2,
        DoisPares = 3,
        Trinca = 4,
        Sequencia = 5,
        Flush = 6,
        FullHouse = 7,
        Quadra = 8,
        StraightFlush = 9,
        RoyalFlush = 10
    }

    // --- VALUE OBJECT: CARTA ---
    public class Carta : IComparable<Carta>
    {
        public int Valor { get; } // 2 a 14 (A = 14)
        public Naipe Naipe { get; }
        public string SimboloOriginal { get; }

        public Carta(string representacao)
        {
            if (string.IsNullOrWhiteSpace(representacao) || representacao.Length < 2 || representacao.Length > 3)
                throw new ArgumentException($"Carta inválida: '{representacao}'");

            string parteValor = representacao.Substring(0, representacao.Length - 1).ToUpper();
            char parteNaipe = char.ToUpper(representacao.Last());

            Valor = parteValor switch
            {
                "2" => 2,
                "3" => 3,
                "4" => 4,
                "5" => 5,
                "6" => 6,
                "7" => 7,
                "8" => 8,
                "9" => 9,
                "D" or "10" => 10,
                "J" => 11,
                "Q" => 12,
                "K" => 13,
                "A" => 14,
                _ => throw new ArgumentException($"Valor de carta inválido: '{parteValor}'")
            };

            Naipe = parteNaipe switch
            {
                'O' => Naipe.Ouro,
                'C' => Naipe.Copas,
                'E' => Naipe.Espadas,
                'P' => Naipe.Paus,
                _ => throw new ArgumentException($"Naipe inválido: '{parteNaipe}'")
            };

            SimboloOriginal = representacao.ToUpper();
        }

        public int CompareTo(Carta? other)
        {
            if (other == null) return 1;
            return this.Valor.CompareTo(other.Valor);
        }

        public override bool Equals(object? obj) => obj is Carta outra && Valor == outra.Valor && Naipe == outra.Naipe;
        public override int GetHashCode() => HashCode.Combine(Valor, Naipe);
    }

    // --- AGREGADO / VALUE OBJECT: MÃO DE POKER ---
    public class MaoPoker
    {
        public List<Carta> Cartas { get; }
        public RankingMao Ranking { get; private set; }
        public List<int> CriterioDesempate { get; private set; } = new();

        public MaoPoker(string entradaCartas)
        {
            var pedacos = entradaCartas.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (pedacos.Length != 5)
                throw new ArgumentException("A mão deve conter exatamente cinco cartas.");

            Cartas = pedacos.Select(p => new Carta(p)).ToList();

            ValidarCartasUnicas();
            AvaliarMao();
        }

        private void ValidarCartasUnicas()
        {
            var duplicadas = Cartas.GroupBy(c => new { c.Valor, c.Naipe }).Any(g => g.Count() > 1);
            if (duplicadas)
                throw new InvalidOperationException("A mão contém cartas repetidas.");
        }

        private void AvaliarMao()
        {
            // Ordena as cartas da maior para a menor por padrão
            var cartasOrdenadas = Cartas.OrderByDescending(c => c.Valor).ToList();

            bool mesmoNaipe = cartasOrdenadas.All(c => c.Naipe == cartasOrdenadas[0].Naipe);
            
            // Verifica sequência
            bool eSequencia = false;
            if (cartasOrdenadas.DistinctBy(c => c.Valor).Count() == 5)
            {
                int maior = cartasOrdenadas[0].Valor;
                int menor = cartasOrdenadas[4].Valor;
                if (maior - menor == 4)
                {
                    eSequencia = true;
                }
            }

            // Agrupa por contagem de valores (ex: 3 do tipo X, 2 do tipo Y)
            var grupos = cartasOrdenadas
                .GroupBy(c => c.Valor)
                .Select(g => new { Valor = g.Key, Contagem = g.Count() })
                .OrderByDescending(g => g.Contagem)
                .ThenByDescending(g => g.Valor)
                .ToList();

            // Identificação dos rankings
            if (eSequencia && mesmoNaipe)
            {
                if (cartasOrdenadas[0].Valor == 14 && cartasOrdenadas[4].Valor == 10)
                {
                    Ranking = RankingMao.RoyalFlush;
                    CriterioDesempate = new(); // Sempre empate
                }
                else
                {
                    Ranking = RankingMao.StraightFlush;
                    CriterioDesempate = new() { cartasOrdenadas[0].Valor };
                }
            }
            else if (grupos[0].Contagem == 4)
            {
                Ranking = RankingMao.Quadra;
                CriterioDesempate = new() { grupos[0].Valor, grupos[1].Valor };
            }
            else if (grupos[0].Contagem == 3 && grupos[1].Contagem == 2)
            {
                Ranking = RankingMao.FullHouse;
                CriterioDesempate = new() { grupos[0].Valor, grupos[1].Valor };
            }
            else if (mesmoNaipe)
            {
                Ranking = RankingMao.Flush;
                CriterioDesempate = cartasOrdenadas.Select(c => c.Valor).ToList();
            }
            else if (eSequencia)
            {
                Ranking = RankingMao.Sequencia;
                CriterioDesempate = new() { cartasOrdenadas[0].Valor };
            }
            else if (grupos[0].Contagem == 3)
            {
                Ranking = RankingMao.Trinca;
                CriterioDesempate = new() { grupos[0].Valor, grupos[1].Valor, grupos[2].Valor };
            }
            else if (grupos[0].Contagem == 2 && grupos[1].Contagem == 2)
            {
                Ranking = RankingMao.DoisPares;
                CriterioDesempate = new() { grupos[0].Valor, grupos[1].Valor, grupos[2].Valor };
            }
            else if (grupos[0].Contagem == 2)
            {
                Ranking = RankingMao.UmPar;
                CriterioDesempate = new() { grupos[0].Valor, grupos[1].Valor, grupos[2].Valor, grupos[3].Valor };
            }
            else
            {
                Ranking = RankingMao.CartaAlta;
                CriterioDesempate = cartasOrdenadas.Select(c => c.Valor).ToList();
            }
        }

        public string ObterDescricaoFormatada()
        {
            return Ranking switch
            {
                RankingMao.CartaAlta => $"Carta Alta ({ObterNomeCarta(CriterioDesempate[0])})",
                RankingMao.UmPar => $"Par de {ObterNomeCarta(CriterioDesempate[0])}",
                RankingMao.DoisPares => $"Dois Pares ({ObterNomeCarta(CriterioDesempate[0])} e {ObterNomeCarta(CriterioDesempate[1])})",
                RankingMao.Trinca => $"Trinca de {ObterNomeCarta(CriterioDesempate[0])}",
                RankingMao.Sequencia => $"Sequência (até {ObterNomeCarta(CriterioDesempate[0])})",
                RankingMao.Flush => "Flush",
                RankingMao.FullHouse => $"Full House com três {ObterNomeCarta(CriterioDesempate[0])}",
                RankingMao.Quadra => $"Quadra de {ObterNomeCarta(CriterioDesempate[0])}",
                RankingMao.StraightFlush => "Straight Flush",
                RankingMao.RoyalFlush => "Royal Flush",
                _ => Ranking.ToString()
            };
        }

        private string ObterNomeCarta(int valor) => valor switch
        {
            14 => "Ás",
            13 => "Rei",
            12 => "Dama",
            11 => "Valete",
            10 => "Dez",
            _ => valor.ToString()
        };
    }

    // --- DOMAIN SERVICE: AVALIADOR DE JOGO ---
    public class ValidadorMaoPoker
    {
        public static string DeterminarVencedor(string maoJogador1, string maoJogador2, HashSet<string>? cartasGlobalUsadas = null)
        {
            MaoPoker j1, j2;
            try
            {
                j1 = new MaoPoker(maoJogador1);
                j2 = new MaoPoker(maoJogador2);

                // Regra: Mão não pode compartilhar carta entre os jogadores
                var todasCartas = j1.Cartas.Concat(j2.Cartas).Select(c => c.SimboloOriginal).ToList();
                if (todasCartas.Distinct().Count() != 10)
                {
                    return "Erro (cartas compartilhadas entre os jogadores)";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ({ex.Message.ToLower()})";
            }

            // Comparação de Rankings
            if (j1.Ranking > j2.Ranking)
                return $"Jogador 1 ({j1.ObterDescricaoFormatada()} vence)";
            if (j2.Ranking > j1.Ranking)
                return $"Jogador 2 ({j2.ObterDescricaoFormatada()} vence)";

            // Mesma classificação: Aplicar regras de desempate
            if (j1.Ranking == RankingMao.RoyalFlush)
                return "Empate";

            for (int i = 0; i < j1.CriterioDesempate.Count; i++)
            {
                if (j1.CriterioDesempate[i] > j2.CriterioDesempate[i])
                    return $"Jogador 1 ({j1.ObterDescricaoFormatada()})";
                if (j2.CriterioDesempate[i] > j1.CriterioDesempate[i])
                    return $"Jogador 2 ({j2.ObterDescricaoFormatada()})";
            }

            return "Empate";
        }
    }
}