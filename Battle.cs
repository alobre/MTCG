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
            //START BATTLE
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
            while (match_running)
            {
                if (round < 100)
                {
                    Console.WriteLine($"ROUND #{round}");
                    Console.WriteLine($"{d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} DAMAGE VS {d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} DAMAGE");
                    // IN CASE OF DRAW: REMOVE BOTH CARDS FROM PLAY
                    if (d1.DeckCards[0].damage == d2.DeckCards[0].damage)
                    {
                        Console.WriteLine($"{d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} DAMAGE EQUALS {d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} DAMAGE");
                        Console.WriteLine("THIS TURN IS A DRAW!");
                        d1.DeckCards.RemoveAt(0);
                        d2.DeckCards.RemoveAt(0);
                        Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                        Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                    }
                    // IN CASE OF USER1 CARD WINS: ADD DEFEATED CARD TO DECK1 AND REMOVE IT FROM DECK2
                    if (d1.DeckCards[0].damage > d2.DeckCards[0].damage)
                    {
                        Console.WriteLine($"{d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} DAMAGE DEFEATS {d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} DAMAGE");
                        Console.WriteLine($"{match.user1.username} WINS THIS TURN!");
                        d1.DeckCards.Add(d2.DeckCards[0]);
                        d2.DeckCards.RemoveAt(0);
                        Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                        Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                    }
                    // IN CASE OF USER2 CARD WINS: ADD DEFEATED CARD TO DECK2 AND REMOVE IT FROM DECK1
                    if (d1.DeckCards[0].damage < d2.DeckCards[0].damage)
                    {
                        Console.WriteLine($"{d2.DeckCards[0].name} WITH {d2.DeckCards[0].damage} DAMAGE DEFEATS {d1.DeckCards[0].name} WITH {d1.DeckCards[0].damage} DAMAGE");
                        Console.WriteLine($"{match.user2.username} WINS THIS TURN!");
                        d2.DeckCards.Add(d1.DeckCards[0]);
                        d1.DeckCards.RemoveAt(0);
                        Console.WriteLine($"{match.user1.username} HAS {d1.DeckCards.Count} CARDS LEFT");
                        Console.WriteLine($"{match.user2.username} HAS {d2.DeckCards.Count} CARDS LEFT");
                    }
                    // IF D1 HAS NO CARDS: USER2 WINS
                    if (d1.DeckCards.Count == 0 && d2.DeckCards.Count > 0)
                    {
                        Console.WriteLine($"{match.user2.username} WINS THIS GAME IN {round} ROUNDS. GGCHEN!");
                        match.user2.wins++;
                        match.user1.losses++;
                        match_running = false;
                    }
                    // IF D2 HAS NO CARDS: USER1 WINS
                    if (d2.DeckCards.Count == 0 && d1.DeckCards.Count > 0)
                    {
                        Console.WriteLine($"{match.user1.username} WINS THIS GAME IN {round} ROUNDS. GGCHEN!");
                        match.user1.wins++;
                        match.user2.losses++;
                        match_running = false;
                    }
                    round++;
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
                    Console.WriteLine(card.cid);
                    if (card.cid == cid) d.DeckCards.Add(card);
                }
            }
            return d.DeckCards;
        }

    }
}
