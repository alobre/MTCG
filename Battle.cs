using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG
{
    public class Battle
    {
        public async Task<Match> StartMatch(Match match)
        {
            // FETCH CARDPOOL
            // GetCardPool();
            // SHUFFLE DECKS
            match.user1.deck = ShuffleDeck(match.user1.deck);
            match.user2.deck = ShuffleDeck(match.user2.deck);
            // START BATTLE
            match = await BattleStarted(match);

            return match;
        }
        async Task<Match> BattleStarted(Match match)
        {
            // EXTRACT DECKS
            int[] deck1 = match.user1.deck;
            int[] deck2 = match.user2.deck;

            var CP = await GetCardPool();
            // CONVERT ARRAY OF INTEGERS INTO LIST OF CARDS WHICH CONTAINS INFO LIKE DAMAGE, ELEMENT, ...
            CardPool d1 = new CardPool();
            CardPool d2 = new CardPool();
            d1.DeckCards = GetDetailedDeck(deck1, CP);
            d2.DeckCards = GetDetailedDeck(deck2, CP);

            // START BATTLE
            int round = 1;
            bool match_running = true;
            ConsoleColor[] colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));

            while (match_running)
            {
                if (round < 100)
                {
                    Console.ForegroundColor = colors[14]; // YELLOW
                    Console.WriteLine($"ROUND #{round}");
                    Console.ForegroundColor = colors[15]; // WHITE

                    if(d1.DeckCards.Count > 0 && d2.DeckCards.Count > 0)
                    {
                        int RoundWinner = TwoCardsBattle(d1.DeckCards[0], d2.DeckCards[0]);
                        // IN CASE OF DRAW: REMOVE BOTH CARDS FROM 
                        if (RoundWinner == 0)
                        {
                            Console.ForegroundColor = colors[6]; // DARKGREEN
                            Console.WriteLine($"{d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} {d1.DeckCards[0].element}-DAMAGE EQUALS {d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} {d2.DeckCards[0].element}-DAMAGE");
                            Console.WriteLine("THIS TURN IS A DRAW!");
                            Console.ForegroundColor = colors[15];// WHITE

                            d1.DeckCards.RemoveAt(0);
                            d2.DeckCards.RemoveAt(0);

                            Console.ForegroundColor = colors[8]; // DARKGREY
                            Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                            Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                            Console.ForegroundColor = colors[15]; // WHITE
                        }
                        // IN CASE OF USER1 CARD WINS: ADD DEFEATED CARD TO DECK1 AND REMOVE IT FROM DECK2
                        if (RoundWinner == 1)
                        {
                            Console.ForegroundColor = colors[2]; // DARKGREEN
                            Console.WriteLine($"{d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} {d1.DeckCards[0].element}-DAMAGE DEFEATS {d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} {d2.DeckCards[0].element}-DAMAGE");
                            Console.WriteLine($"{match.user1.username} WINS THIS TURN!");
                            Console.ForegroundColor = colors[15]; // WHITE
                            d1.DeckCards.Add(d2.DeckCards[0]);
                            d2.DeckCards.RemoveAt(0);

                            Console.ForegroundColor = colors[8]; // DARKGREY
                            Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                            Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                            Console.ForegroundColor = colors[15]; // WHITE
                        }
                        // IN CASE OF USER2 CARD WINS: ADD DEFEATED CARD TO DECK2 AND REMOVE IT FROM DECK1
                        if (RoundWinner == 2)
                        {
                            Console.ForegroundColor = colors[2]; // DARKGREEN
                            Console.WriteLine($"{d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} {d2.DeckCards[0].element}-DAMAGE DEFEATS {d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} {d1.DeckCards[0].element}-DAMAGE");
                            Console.WriteLine($"{match.user2.username} WINS THIS TURN!");
                            Console.ForegroundColor = colors[15]; // WHITE

                            d2.DeckCards.Add(d1.DeckCards[0]);
                            d1.DeckCards.RemoveAt(0);

                            Console.ForegroundColor = colors[8]; // DARKGREY
                            Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                            Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                            Console.ForegroundColor = colors[15]; // WHITE
                        }
                        round++;
                    }
                    else
                    {
                        // IF D1 HAS NO CARDS: USER2 WINS
                        if (d1.DeckCards.Count == 0 && d2.DeckCards.Count > 0)
                        {
                            Console.ForegroundColor = colors[10]; // GREEN
                            Console.WriteLine($"{match.user2.username} WINS THIS GAME IN {round} ROUNDS. GGCHEN!");
                            Console.ForegroundColor = colors[15]; // WHITE

                            match.user2.wins++;
                            match.user1.losses++;

                            match.user2.elo += 20;
                            match.user1.elo -= 15;

                            match_running = false;
                        }
                        // IF D2 HAS NO CARDS: USER1 WINS
                        if (d2.DeckCards.Count == 0 && d1.DeckCards.Count > 0)
                        {
                            Console.ForegroundColor = colors[10]; // GREEN
                            Console.WriteLine($"{match.user1.username} WINS THIS GAME IN {round} ROUNDS. GGCHEN!");
                            Console.ForegroundColor = colors[15]; // WHITE

                            match.user1.wins++;
                            match.user2.losses++;

                            match.user1.elo += 20;
                            match.user2.elo -= 15;

                            match_running = false;
                        }
                        // IF BOTH DECKS HAVE NO CARDS
                        if (d1.DeckCards.Count == 0 && d2.DeckCards.Count == 0)
                        {
                            Console.ForegroundColor = colors[10]; // GREEN
                            Console.WriteLine($"{match.user1.username} AND {match.user2.username} HAVE NO CARDS LEFT. THIS GAME IS A DRAW. GGCHEN!");
                            Console.ForegroundColor = colors[15]; // WHITE
                            match.user1.draw++;
                            match.user2.draw++;
                            match_running = false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("DRAW! REACHED ROUND LIMIT OF 100.");
                    match.user1.draw++;
                    match.user2.draw++;
                    match_running = false;
                }
            }
            return match;
        }
        public int TwoCardsBattle(Card card1, Card card2)
        {
            // NO SPELLS INVOLVED
            if(card1.card_type == "monster" && card2.card_type == "monster")
            {
                if (card1.damage > card2.damage) return 1;
                if (card2.damage > card1.damage) return 2;
                if (card1.damage == card2.damage) return 0;
            }
            // SPELLS INVOLVED
            if(card1.card_type == "spell" || card2.card_type == "spell")
            {
                double c1 = card1.damage;
                if(c1 * DamagemultiplicatorCalculator(card1, card2) > card2.damage)
                {
                    return 1;
                }
                if(c1 * DamagemultiplicatorCalculator(card1, card2) < card2.damage)
                {
                    return 2;
                }
                if (c1 * DamagemultiplicatorCalculator(card1, card2) == card2.damage)
                {
                    return 0;
                }
            }
            return 0;
        }
        double DamagemultiplicatorCalculator(Card card1, Card card2)
        {
            if (card1.card_type == "spell" || card2.card_type == "spell")
            {
                switch (card1.element)
                {
                    case "water":
                        switch (card2.element)
                        {
                            case "water":
                                return 1;
                            case "fire":
                                return 2;
                            case "normal":
                                return 0.5;
                        }
                        break;
                    case "fire":
                        switch (card2.element)
                        {
                            case "water":
                                return 0.5;
                            case "fire":
                                return 1;
                            case "normal":
                                return 2;
                        }
                        break;
                    case "normal":
                        switch (card2.element)
                        {
                            case "water":
                                return 2;
                            case "fire":
                                return 0.5;
                            case "normal":
                                return 1;
                        }
                        break;
                    default:
                        break;
                }
            }
            return 1;
        }
        int[] ShuffleDeck(int[] deck)
        {
            Random random = new Random();
            deck = deck.OrderBy(x => random.Next()).ToArray();
            return deck;
        }
        public async Task<List<Card>> GetCardPool()
        {
            CardPool.AllCards.Clear();
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            var cmd = new NpgsqlCommand("", conn);
            cmd.CommandText = "SELECT * FROM card_pool";
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    Card card = new Card();
                    card.cid = (int)reader["cid"];
                    card.card_type = (string)reader["card_type"];
                    card.name = (string)reader["name"];
                    card.element = (string)reader["element"];
                    card.damage = (int)reader["damage"];
                    CardPool.AllCards.Add(card);
                }
            return CardPool.AllCards;
        }
        public List<Card> GetDetailedDeck(int[] deck, List<Card> CP)
        {
            CardPool d = new CardPool();
            foreach (int cid in deck)
            {
                foreach (var card in CP)
                {
                    if (card.cid == cid) d.DeckCards.Add(card);
                }
            }
            return d.DeckCards;
        }

    }
}
