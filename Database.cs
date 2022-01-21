using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG
{
    public class Database
    {
        static object commandlock = new object();
        public async Task<Npgsql.NpgsqlConnection> ConnectDB(string server, string username, string password, string database)
        {
            var connString = $"Host={server};Username={username};Password={password};Database={database}";

            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn;
        }
        public async void Register(string username, string password, Npgsql.NpgsqlConnection conn)
        {
            var cmd = new NpgsqlCommand("", conn);
            int uid = await GetUIDByUsername(username, cmd);
            if (uid == 0)
            {
                lock (commandlock)
                {
                    cmd.CommandText = $"INSERT INTO users(username, password) VALUES(@username, @password)";
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.Prepare();
                    cmd.ExecuteNonQueryAsync();
                }
                //Thread.Sleep(10);
                lock (commandlock)
                {
                    uid = GetUIDByUsername(username, cmd).Result;
                }
                SetUserBalance(uid, cmd);
                SetAccessToken(uid, username, password, cmd);
                //Console.WriteLine(await GetUserBalance(username, cmd));
            }
            else
            {
                Console.WriteLine("Username is already taken. Please choose a different username!");
            }
        }
        public async Task<string> LoginByCredentials(string username, string password, Npgsql.NpgsqlConnection conn)
        {
            int uid;
            string access_token, due_date;
            var cmd = new NpgsqlCommand($"SELECT u.uid, u.username, act.access_token, act.due_date FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username = @username AND u.password = @password", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    uid = (int)reader["uid"];
                    access_token = (string)reader["access_token"];
                    due_date = reader["due_date"].ToString();
                    return $"{{\"msg\":\"Login successful!\", \"uid\": {uid}, \"access_token\": \"{access_token}\", \"success\": true}}";
                }
            return $"{{\"msg\":\"Login failed. Please check your credentials.\", \"success\": false}}";

        }
        public async Task<bool> ValidateToken(string access_token, Npgsql.NpgsqlCommand cmd)
        {
            string due_date;
            int uid;
            cmd.CommandText = $"SELECT u.uid, u.username, act.due_date FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE act.access_token = @access_token";
            cmd.Parameters.AddWithValue("access_token", access_token);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    uid = (int)reader["uid"];
                    due_date = reader["due_date"].ToString();
                    if (DateTime.Now <= DateTime.Parse(due_date))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            return false;
        }
        public static async Task<string> GetAccessToken(string username, string password, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT act.access_token FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username == @username && u.password = @password";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    return (string)reader["access_token"];
                }
            return "credentials wrong";
        }
        /*public static async Task<string> ExtendAccessToken(string username, string password, Npgsql.NpgsqlCommand cmd)
		{
			cmd.CommandText = $"SELECT act.\"accessToken\" FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username == {username} && u.password = {password}";
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
				{
					return (string)reader["accessToken"];
				}
			return "credentials wrong";
		}*/
        public async Task<int> GetUIDByUsername(string username, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT uid FROM users WHERE username = @username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                try
                {
                    while (await reader.ReadAsync())
                        return reader.GetInt32(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                finally
                {
                    reader.Close();
                }

            }

            return 0;
        }
        public void SetUserBalance(int uid, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                cmd.CommandText = $"INSERT INTO balances(uid, coins) VALUES(@uid, 20)";
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.Prepare();
                cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> GetUserBalance(string username, Npgsql.NpgsqlCommand cmd)
        {
            int uid = await GetUIDByUsername(username, cmd);
            cmd.CommandText = $"SELECT coins FROM balances WHERE uid = '(@uid)'";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    return reader.GetInt32(0);
            return 0;
        }
        public void SetAccessToken(int uid, string username, string password, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                string hash = GetStringSha256Hash(username + password);
                cmd.CommandText = $"INSERT INTO access_tokens(uid, access_token, due_date) VALUES(@uid, @hash, '2022-01-01')";
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.Parameters.AddWithValue("hash", hash);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
        public async Task<int> GetAccessToken(string username, Npgsql.NpgsqlCommand cmd)
        {
            int uid = await GetUIDByUsername(username, cmd);
            cmd.CommandText = $"SELECT coins FROM balances WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    return reader.GetInt32(0);
            return 0;
        }
        public async Task<int> GetCoinsByUID(int uid, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT * FROM balances WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    return (int)reader["coins"];
            return 0;
        }
        public async Task<int> OpenPack(int uid, Npgsql.NpgsqlCommand cmd)
        {
            //reduce balance by 5
            int newCoins = await GetCoinsByUID(uid, cmd) - 5;
            lock (commandlock)
            {
                cmd.CommandText = $"UPDATE balances SET coins = @newCoins WHERE uid = @uid";
                cmd.Parameters.AddWithValue("newCoins", newCoins);
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return newCoins;
            }

        }
        public async Task<int[]> GetRandom(Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT count(*) FROM card_pool";
            int[] cardIdArray = new int[5];
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    long totalCount = (long)reader["count"];
                    for (int i = 0; i < cardIdArray.Length; i++)
                    {
                        Random random = new Random();
                        cardIdArray[i] = random.Next(1, (int)totalCount);
                    }
                }
            return cardIdArray;
        }
        public async Task<Card> GetCardByCID(int cid, Npgsql.NpgsqlCommand cmd)
        {
            Console.WriteLine(cid);
            cmd.CommandText = $"SELECT cid, card_type, name, element, damage FROM card_pool WHERE cid = @cid";
            cmd.Parameters.AddWithValue("cid", cid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    Card card = new Card
                    {
                        cid = (int)reader["cid"],
                        card_type = (string)reader["card_type"],
                        name = (string)reader["name"],
                        element = (string)reader["element"],
                        damage = (int)reader["damage"]
                    };
                    return card;
                }
            return new Card();
        }
        public async void AssignCardsToUser(int uid, int[] cardIdArray, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                for (int i = 0; i < cardIdArray.Length; i++)
                {
                    int curr = cardIdArray[i];
                    cmd.CommandText = $"INSERT INTO collections(uid, cid) VALUES (@uid, @cid)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("uid", uid);
                    cmd.Parameters.AddWithValue("cid", curr);
                    cmd.Prepare();
                    var reader = cmd.ExecuteNonQuery();
                }
            }

        }

        public async Task<List<Card>> CreateCardList(int[] cardIdArray, Npgsql.NpgsqlCommand cmd)
        {
            List<Card> CardList = new List<Card>();
            cmd.CommandText = $"SELECT cid, card_type, name, element, damage FROM card_pool WHERE cid in (@1, @2, @3, @4, @5)";
            for (int i = 0; i < cardIdArray.Length; i++)
            {
                cmd.Parameters.AddWithValue($"{i + 1}", cardIdArray[i]);
            }
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    Card card = new Card
                    {
                        cid = (int)reader["cid"],
                        card_type = (string)reader["card_type"],
                        name = (string)reader["name"],
                        element = (string)reader["element"],
                        damage = (int)reader["damage"]
                    };
                    CardList.Add(card);
                }
            return CardList;
        }
        public async Task<List<Card>> GetUsersCollection(int uid, Npgsql.NpgsqlCommand cmd)
        {
            List<Card> CardList = new List<Card>();
            cmd.CommandText = $"SELECT cid, card_type, name, element, damage FROM card_pool WHERE cid in (SELECT cid FROM collections WHERE uid = @uid)";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    Card card = new Card
                    {
                        cid = (int)reader["cid"],
                        card_type = (string)reader["card_type"],
                        name = (string)reader["name"],
                        element = (string)reader["element"],
                        damage = (int)reader["damage"]
                    };
                    CardList.Add(card);
                }
            return CardList;
        }
        public async Task<UserProfile> GetUserProfile(int uid, Npgsql.NpgsqlCommand cmd)
        {
            UserProfile user = new UserProfile();
            cmd.CommandText = "SELECT user_profile.uid, user_profile.elo, user_profile.deck, user_profile.wins, user_profile.losses, user_profile.draw, users.username  FROM user_profile JOIN users ON user_profile.uid = users.uid WHERE user_profile.uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {

                    user.uid = (int)reader["uid"];
                    user.username = (string)reader["username"];
                    user.elo = (int)reader["elo"];
                    user.deck = (int[])reader["deck"];
                    user.wins = (int)reader["wins"];
                    user.losses = (int)reader["losses"];
                    user.draw = (int)reader["draw"];
                }
            return user;

        }
        public void SyncUserProfile(UserProfile user, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                cmd.CommandText = "UPDATE user_profile SET elo = @elo, wins = @wins, losses = @losses, draw = @draw WHERE uid = @uid";
                cmd.Parameters.AddWithValue("elo", user.elo); // ELO

                cmd.Parameters.AddWithValue("wins", user.wins); // WINS
                cmd.Parameters.AddWithValue("losses", user.losses); // LOSSES
                cmd.Parameters.AddWithValue("draw", user.draw); // DRAW
                cmd.Parameters.AddWithValue("uid", user.uid); // DRAW
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

        }

        public async Task<string> CheckAndCreateUserProfile(int uid, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = "SELECT uid FROM user_profile WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            bool UserProfileUidExists = false;
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    if ((int)reader["uid"] == uid) UserProfileUidExists = true;
                }
            if (UserProfileUidExists == true) return "User Profile already Exists";
            else
            {
                lock (commandlock)
                {
                    cmd.CommandText = "INSERT INTO user_profile (uid, elo, wins, losses, draw) VALUES (@uid, 100, 0,0,0)";
                    cmd.Parameters.AddWithValue("uid", uid);
                    cmd.ExecuteNonQueryAsync();
                    return "User Profile did not exist before and was created";
                }

            }

        }
        public async Task<int[]> SetDeck(int uid, UserProfile user, Npgsql.NpgsqlCommand cmd)
        {
            await CheckAndCreateUserProfile(uid, cmd);
            if (CheckIfCountainsDuplicates(user.deck) == false)
            {
                bool DoesUserOwnCards = await CheckIfUserOwnsCards(uid, user.deck, cmd);
                if (DoesUserOwnCards == true)
                {
                    lock (commandlock)
                    {
                        cmd.CommandText = "UPDATE user_profile SET deck = ARRAY[@cid1,@cid2,@cid3,@cid4] WHERE uid = @uid";
                        cmd.Parameters.AddWithValue("uid", uid);
                        for (int i = 0; i < user.deck.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"cid{i + 1}", user.deck[i]);
                        }
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                        return user.deck;
                    }
                }
                else
                {
                    int[] emptyArray = new int[0];
                    return emptyArray;
                }
            }
            else
            {
                int[] emptyArray = new int[0];
                return emptyArray;
            }
        }
        public bool CheckIfCountainsDuplicates(int[] deck)
        {
            var dict = new Dictionary<int, int>();
            foreach (var card in deck)
            {
                if (!dict.ContainsKey(card))
                {
                    dict.Add(card, 0);
                }
                dict[card]++;
            }
            bool hasDouplicates;
            if (dict.Count == 4)
            {
                hasDouplicates = false;
            }
            else
            {
                hasDouplicates = true;
            }
            return hasDouplicates;
        }
        public async Task<bool> CheckIfUserOwnsCards(int uid, int[] deck, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT cid FROM collections WHERE uid = @uid AND cid IN (@cid1, @cid2, @cid3, @cid4)";
            cmd.Parameters.AddWithValue("uid", uid);
            for (int i = 0; i < deck.Length; i++)
            {
                cmd.Parameters.AddWithValue($"cid{i + 1}", deck[i]);
            }
            cmd.Prepare();
            bool UserOwnsCard = true;

            var dict = new Dictionary<int, int>();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    int card = (int)reader["cid"];
                    if (!dict.ContainsKey(card))
                    {
                        dict.Add(card, 0);
                    }
                    dict[card]++;
                }
            foreach (var card in deck)
            {
                if (dict.ContainsKey(card) == false) UserOwnsCard = false;
            }
            return UserOwnsCard;
        }
        public async void StartBattle(UserProfile user, Npgsql.NpgsqlCommand cmd)
        {
            // ADD USER TO QUEUE
            AddUserToQueue(user);
            if (Queue.UsersInQueue.Count >= 2)
            {
                Match match_results = await CheckForOpponment(cmd);
                // REMOVE USERS FROM QUEUE
                RemoveUserFromQueue(match_results.user1);
                RemoveUserFromQueue(match_results.user2);
                // SYNC WITH DATABASE
                SyncUserProfile(match_results.user1, cmd);
                SyncUserProfile(match_results.user2, cmd);
            }

        }
        public void AddUserToQueue(UserProfile user)
        {
            Queue.UsersInQueue.Add(user);
        }
        public void RemoveUserFromQueue(UserProfile user)
        {
            Queue.UsersInQueue.Remove(new UserProfile() { uid = user.uid });
        }
        public async Task<Match> CheckForOpponment(Npgsql.NpgsqlCommand cmd)
        {
            if (Queue.UsersInQueue.Count >= 2)
            {
                /*
				for(int i = 0; i < Q.UsersInQueue.Count; i++)
                {
					Q.UsersInQueue[i].elo
                }
				*/
                Battle battle = new Battle();
                Match match = new Match(); // ADD USERS TO MATCH
                match.user1 = Queue.UsersInQueue[0];
                match.user2 = Queue.UsersInQueue[1];
                Match match_results = await battle.StartMatch(match); // START BATTLE
                return match_results;
            }
            else return null;
        }
        public Tradeoffers GetTradeoffers(int uid, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                Tradeoffers tradeoffers = new Tradeoffers();
                cmd.CommandText = "SELECT * FROM tradeoffers WHERE recipient_uid = @uid";
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.Prepare();

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        Tradeoffer to = new Tradeoffer();
                        to.tradeoffer_id = (int)reader["tradeoffer_id"];
                        to.sender_uid = (int)reader["sender_uid"];
                        to.recipient_uid = (int)reader["recipient_uid"];
                        to.i_receive = (int[])reader["i_receive"];
                        to.u_receive = (int[])reader["u_receive"];
                        to.status = (string)reader["status"];

                        tradeoffers.AllTradeoffers.Add(to);
                    }
                return tradeoffers;
            }

        }
        public Tradeoffer GetTradeofferByTradeoffer_id(int tradeoffer_id, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                Tradeoffer tradeoffer = new Tradeoffer();
                cmd.CommandText = "SELECT * FROM tradeoffers WHERE tradeoffer_id = @tradeoffer_id";
                cmd.Parameters.AddWithValue("tradeoffer_id", tradeoffer_id);
                cmd.Prepare();

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        tradeoffer.tradeoffer_id = (int)reader["tradeoffer_id"];
                        tradeoffer.sender_uid = (int)reader["sender_uid"];
                        tradeoffer.recipient_uid = (int)reader["recipient_uid"];
                        tradeoffer.i_receive = (int[])reader["i_receive"];
                        tradeoffer.u_receive = (int[])reader["u_receive"];
                        tradeoffer.status = (string)reader["status"];
                    }
                return tradeoffer;
            }

        }
        public async Task<string> AcceptTradeoffer(int uid, int tradeoffer_id, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = "UPDATE tradeoffers SET status = @status WHERE tradeoffer_id = @tradeoffer_id AND recipient_uid = @uid";
            string status = "accepted";
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Parameters.AddWithValue("tradeoffer_id", tradeoffer_id);
            cmd.Prepare();

            await cmd.ExecuteNonQueryAsync();

            Tradeoffer tradeoffer = GetTradeofferByTradeoffer_id(tradeoffer_id, cmd);
            // CHECK IF USER WOULD RECEIVE CARDS, ELSE IT IS NO NECESSARY TO SWAP
            if(tradeoffer.u_receive.Length > 0) ChangeCardOwner(tradeoffer.recipient_uid, tradeoffer.sender_uid, tradeoffer.u_receive, cmd);
            if(tradeoffer.i_receive.Length > 0) ChangeCardOwner(tradeoffer.sender_uid, tradeoffer.recipient_uid, tradeoffer.i_receive, cmd);

            return "Tradeoffer successfully accepted!";
        }
        // SWAP CARDS TO THE NEW OWNER
        public string ChangeCardOwner(int uid, int sender_uid, int[] cards, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                string cardsCountString = "";
                for (int i = 1; i <= cards.Length; i++)
                {
                    if(i != cards.Length) cardsCountString += $"@c{i},";
                    else cardsCountString += $"@c{i}";
                }
                string commandText = $"UPDATE collections SET uid = @uid WHERE cid IN ({cardsCountString}) AND uid = @sender_uid";
                cmd.CommandText = commandText;
                cmd.Parameters.AddWithValue("cards", cards);
                for (int i = 0; i < cards.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"c{i+1}", cards[i]);
                }
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.Parameters.AddWithValue("sender_uid", sender_uid);
                cmd.Prepare();

                cmd.ExecuteNonQuery();

                return "Tradeoffer successfully accepted!";
            }

        }
        public async Task<string> DeclineTradeoffer(int uid, int tradeoffer_id, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = "UPDATE tradeoffers SET status = @status WHERE tradeoffer_id = @tradeoffer_id";
            string status = "declined";
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("tradeoffer_id", tradeoffer_id);
            cmd.Prepare();

            await cmd.ExecuteNonQueryAsync();

            return "Tradeoffer successfully declined!";
        }
        public string CreateTradeoffer(int sender_uid, int recipient_uid, int[] i_receive, int[] u_receive, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                cmd.CommandText = "INSERT INTO tradeoffers (sender_uid, recipient_uid, i_receive, u_receive, status) VALUES (@sender_uid, @recipient_uid, @i_receive, @u_receive, @status)";

                cmd.Parameters.AddWithValue("sender_uid", sender_uid);
                cmd.Parameters.AddWithValue("recipient_uid", recipient_uid);
                cmd.Parameters.AddWithValue("i_receive", i_receive);
                cmd.Parameters.AddWithValue("u_receive", u_receive);
                string status = "pending";
                cmd.Parameters.AddWithValue("status", status);
                cmd.Prepare();

                cmd.ExecuteNonQuery();

                return "Tradeoffer successfully created!";
            }

        }
        public async Task<bool> CheckIfUserOwnsCard(int[] cards, int uid, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = "SELECT cid FROM collections WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            cmd.Prepare();
            bool UserOwnsCard = true;

            var dict = new Dictionary<int, int>();
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    int card = (int)reader["cid"];
                    if (!dict.ContainsKey(card))
                    {
                        dict.Add(card, 0);
                    }
                    dict[card]++;
                }
            foreach (var card in cards)
            {
                if (dict.ContainsKey(card) == false) UserOwnsCard = false;
            }
            return UserOwnsCard;
        }
        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}
