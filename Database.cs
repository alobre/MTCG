using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG
{
	class Database
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
			var cmd = new NpgsqlCommand("",conn);
			int uid = await GetUIDByUsername(username, cmd);
			if (uid == 0)
            {
					cmd.CommandText = $"INSERT INTO users(username, password) VALUES('{username}', '{password}')";
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
			var cmd = new NpgsqlCommand($"SELECT u.uid, u.username, act.access_token, act.due_date FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username = '(@username)' AND u.password = '(@password)'", conn);
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
					return $"{{msg:\"Login successful!\", uid: {uid}, access_token: \"{access_token}\"}}";
				}
			return $"{{msg:\"Login failed. Please check your credentials.\"}}";

		}
		public async Task<bool> ValidateToken(string access_token, Npgsql.NpgsqlCommand cmd)
		{
			string due_date;
			int uid;
			cmd.CommandText = $"SELECT u.uid, u.username, act.due_date FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE act.access_token = '{access_token}'";
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
			cmd.CommandText = $"SELECT act.access_token FROM users AS u JOIN access_tokens AS act ON u.uid = act.uid WHERE u.username == {username} && u.password = {password}";
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
			cmd.CommandText = $"SELECT uid FROM users WHERE username = '{username}'";
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
					return reader.GetInt32(0);
			return 0;
		}
		public async void SetUserBalance(int uid, Npgsql.NpgsqlCommand cmd)
        {
			cmd.CommandText = $"INSERT INTO balances(uid, coins) VALUES('{uid}', 20)";
				await cmd.ExecuteNonQueryAsync();
		}
		public async Task<int> GetUserBalance(string username, Npgsql.NpgsqlCommand cmd)
		{
			int uid = await GetUIDByUsername(username, cmd);
			cmd.CommandText = $"SELECT coins FROM balances WHERE uid = '{uid}'";
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
					return reader.GetInt32(0);
			return 0;
		}
		public async void SetAccessToken(int uid, string username, string password, Npgsql.NpgsqlCommand cmd)
		{
			string hash = GetStringSha256Hash(username + password);
			cmd.CommandText = $"INSERT INTO access_tokens(uid, access_token, due_date) VALUES('{uid}', '{hash}', '2022-01-01')";
				await cmd.ExecuteNonQueryAsync();
		}
		public async Task<int> GetAccessToken(string username, Npgsql.NpgsqlCommand cmd)
		{
			int uid = await GetUIDByUsername(username, cmd);
			cmd.CommandText = $"SELECT coins FROM balances WHERE uid = '{uid}'";
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
					return reader.GetInt32(0);
			return 0;
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
