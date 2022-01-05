using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG
{
    public class Database
    {
        static object commandlock;
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
                cmd.CommandText = $"INSERT INTO users(username, password) VALUES(@username, @password)";
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);
                await cmd.ExecuteNonQueryAsync();
                //test(cmd);
                uid = await GetUIDByUsername(username, cmd);
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
            bool isValid;
            var cmd = new NpgsqlCommand($"SELECT u.uid, u.username, act.access_token, act.due_date FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username = @username AND u.password = @password", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    uid = (int)reader["uid"];
                    access_token = (string)reader["access_token"];
                    due_date = reader["due_date"].ToString();
                    /*isValid = await ValidateToken(access_token, cmd);
					if (isValid == true)
					{
						return $"{{msg:\"Login successful!\", uid: {uid}, access_token: \"{access_token}\"}}";
					}*/
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
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    return reader.GetInt32(0);
            return 0;
        }
        public void SetUserBalance(int uid, Npgsql.NpgsqlCommand cmd)
        {
            lock (commandlock)
            {
                cmd.CommandText = $"INSERT INTO balances(uid, coins) VALUES(@uid, 20)";
                cmd.Parameters.AddWithValue("uid", uid);
                cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<int> GetUserBalance(string username, Npgsql.NpgsqlCommand cmd)
        {
            int uid = await GetUIDByUsername(username, cmd);
            cmd.CommandText = $"SELECT coins FROM balances WHERE uid = '(@uid)'";
            cmd.Parameters.AddWithValue("uid", uid);
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
                cmd.ExecuteNonQuery();
            }
        }
        public async Task<int> GetAccessToken(string username, Npgsql.NpgsqlCommand cmd)
        {
            int uid = await GetUIDByUsername(username, cmd);
            cmd.CommandText = $"SELECT coins FROM balances WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    return reader.GetInt32(0);
            return 0;
        }
        public async Task<int> GetCoinsByUID(int uid, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT * FROM balances WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
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
                cmd.ExecuteNonQuery();
                return newCoins;
            }

        }
        public async Task<int[]> GetRandom(Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = $"SELECT count(*) FROM card_pool";
            int[] cardIdArray = new int[5];
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
            cmd.CommandText = "SELECT * FROM user_profile WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    user.uid = (int)reader["uid"];
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
                cmd.CommandText = "UPDATE user_profile SET elo = @elo, deck = ARRAY[@cid1,@cid2,@cid3,@cid4], wins = @wins, losses = @losses, draw = @draw WHERE uid = @uid";
                cmd.Parameters.AddWithValue("elo", user.elo); // ELO
                for (int i = 0; i < user.deck.Length; i++) // DECK FOR LOOP
                {
                    cmd.Parameters.AddWithValue($"cid{i + 1}", user.deck[i]);
                }
                cmd.Parameters.AddWithValue("wins", user.wins); // WINS
                cmd.Parameters.AddWithValue("losses", user.losses); // LOSSES
                cmd.Parameters.AddWithValue("draw", user.draw); // DRAW
                cmd.ExecuteNonQuery();
            }

        }

        public async Task<string> CheckAndCreateUserProfile(int uid, Npgsql.NpgsqlCommand cmd)
        {
            cmd.CommandText = "SELECT uid FROM user_profile WHERE uid = @uid";
            cmd.Parameters.AddWithValue("uid", uid);
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
            //Queue Q = AddUserToQueue(user); // ADD USER TO QUEUE
            AddUserToQueue(user);
            if (Queue.UsersInQueue.Count >= 2)
            {
                Match match_results = CheckForOpponment();
                SyncUserProfile(match_results.user1, cmd);
                SyncUserProfile(match_results.user2, cmd);
            }

        }
        public void AddUserToQueue(UserProfile user)
        {
            //Queue Q = new Queue();
            //Q.UsersInQueue.Add(user);
            Queue.UsersInQueue.Add(user);
        }
        public Match CheckForOpponment()
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
                Match match_results = battle.StartMatch(match); // START BATTLE
                return match_results;
            }
            else return null;
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
