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
		public async void SetUserBalance(int uid, Npgsql.NpgsqlCommand cmd)
		{
			cmd.CommandText = $"INSERT INTO balances(uid, coins) VALUES(@uid, 20)";
			cmd.Parameters.AddWithValue("uid", uid);
			await cmd.ExecuteNonQueryAsync();
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
		public async void SetAccessToken(int uid, string username, string password, Npgsql.NpgsqlCommand cmd)
		{
			string hash = GetStringSha256Hash(username + password);
			cmd.CommandText = $"INSERT INTO access_tokens(uid, access_token, due_date) VALUES(@uid, @hash, '2022-01-01')";
			cmd.Parameters.AddWithValue("uid", uid);
			cmd.Parameters.AddWithValue("hash", hash);
			await cmd.ExecuteNonQueryAsync();
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
			cmd.CommandText = $"UPDATE balances SET coins = @newCoins WHERE uid = @uid";
			cmd.Parameters.AddWithValue("newCoins", newCoins);
			cmd.Parameters.AddWithValue("uid", uid);
			cmd.ExecuteNonQuery();
			return newCoins;
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
			for (int i = 0; i < cardIdArray.Length; i++)
			{
				int curr = cardIdArray[i];
				cmd.CommandText = $"INSERT INTO collections(uid, cid) VALUES (@uid, @cid)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("uid", uid);
				cmd.Parameters.AddWithValue("cid", curr);
				var reader = await cmd.ExecuteNonQueryAsync();
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
		public async Task<int[]> SetDeck(int uid, int[] deck, Npgsql.NpgsqlCommand cmd)
		{
			if(CheckIfCountainsDuplicates(deck) == false)
            {
				bool DoesUserOwnCards = await CheckIfUserOwnsCards(uid, deck, cmd);
				if (DoesUserOwnCards == true)
                {
					return deck;
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
			/*
			cmd.CommandText = $"SELECT cid FROM collections WHERE uid = @uid AND cid IN (@cid1, @cid2, @cid3, @cid4)";
			cmd.Parameters.AddWithValue("uid", uid);
			for (int i = 0; i < deck.Length; i++)
			{
				cmd.Parameters.AddWithValue($"cid{i + 1}", deck[i]);
			}
			*/

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
		public async void StartBattle(int uid, UserProfile user, Npgsql.NpgsqlCommand cmd)
		{
			user = await CheckElo(uid, user, cmd);

		}
		public async Task<UserProfile> CheckElo(int uid, UserProfile user, Npgsql.NpgsqlCommand cmd)
		{
			cmd.CommandText = $"SELECT elo FROM elo WHERE uid = @uid";
			cmd.Parameters.AddWithValue("uid", uid);
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
				{
					user.elo = (int)reader["elo"];
				}
			return user;
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
