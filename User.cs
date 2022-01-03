using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Npgsql;

namespace MTCG
{
    public class User
    {
        public static List<User> Alle_User = new List<User>();

        internal credentials Usercredentials = new credentials();

        Stack User_Stack = new Stack();
        Deck User_deck = new Deck();
        public int coins { get; set; } = 20;
        public User(string username,string password)
        {
            if (Alle_User.Find(x => x.Usercredentials.username == username) == null)
            {
                Usercredentials.username = username;
                Usercredentials.password = password;
                string usernameAndPassword = username + password;
                string accessToken = GetStringSha256Hash(usernameAndPassword);
                Console.WriteLine(accessToken);
                Register(username, password);
            }
            else
            {
                
                //throw new Exception();
            }
        }
        public async void Register(string username, string password) 
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            db.Register(username, password, conn);
            conn.Close();
        }
        /*public async void LoginByCredentials(string username, string password)
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            db.LoginByCredentials(username, password, conn);
        }*/
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
