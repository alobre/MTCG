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
			int uid = await GetUIDByUsername(username, conn);
            if(uid == 0)
            {
				await using (var cmd = new NpgsqlCommand($"INSERT INTO users(username, password) VALUES('{username}', '{password}')", conn))
				{
					await cmd.ExecuteNonQueryAsync();
				}
				uid = await GetUIDByUsername(username, conn);
				SetUserBalance(uid, conn);
				SetAccessToken(uid, username, password, conn);
                Console.WriteLine(await GetUserBalance(username, conn));
			}
            else
            {
                Console.WriteLine("Username is already taken. Please choose a different username!");
            }
		}
		public async Task<int> GetUIDByUsername(string username, Npgsql.NpgsqlConnection conn)
		{
			await using (var cmd = new NpgsqlCommand($"SELECT uid FROM users WHERE username = '{username}'", conn))
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
					return reader.GetInt32(0);
			return 0;
		}
		public async void SetUserBalance(int uid, Npgsql.NpgsqlConnection conn)
        {
			await using (var cmd = new NpgsqlCommand($"INSERT INTO balances(uid, coins) VALUES('{uid}', 20)", conn))
			{
				await cmd.ExecuteNonQueryAsync();
			}
		}
		public async Task<int> GetUserBalance(string username, Npgsql.NpgsqlConnection conn)
		{
			int uid = await GetUIDByUsername(username, conn);
			await using (var cmd = new NpgsqlCommand($"SELECT coins FROM balances WHERE uid = '{uid}'", conn))
			await using (var reader = await cmd.ExecuteReaderAsync())
				while (await reader.ReadAsync())
					return reader.GetInt32(0);
			return 0;
		}
		public async void SetAccessToken(int uid, string username, string password, Npgsql.NpgsqlConnection conn)
		{
			string hash = GetStringSha256Hash(username + password);
			await using (var cmd = new NpgsqlCommand($"INSERT INTO access_tokens(uid, acessToken, due_date) VALUES('{uid}', '{hash}', '2022-01-01')", conn))
			{
				await cmd.ExecuteNonQueryAsync();
			}
		}
		public async Task<int> GetAccessToken(string username, Npgsql.NpgsqlConnection conn)
		{
			int uid = await GetUIDByUsername(username, conn);
			await using (var cmd = new NpgsqlCommand($"SELECT coins FROM balances WHERE uid = '{uid}'", conn))
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
