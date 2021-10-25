using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    class DatabaseHandler
    {
        public static async void Register(string username, string password)
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            db.Register(username, password, conn);
            //conn.Close();
        }
        public static async Task<string> Login(string username = "", string password = "", string access_token = "")
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            if(access_token != "")
            {
                var cmd = new Npgsql.NpgsqlCommand("", conn);
                bool isValid = await db.ValidateToken(access_token, cmd);
                if (isValid == true)
                {
                    return "{msg: \"access_token valid!\", sucess: true}";
                }
                else
                {
                    return "{msg: \"access_token not valid! Please Login again!\", sucess: false}";
                }
            } else if(username != "" && password != "" )
            {
                string response = await db.LoginByCredentials(username, password, conn);
                Console.WriteLine(response);
                //conn.Close();
                return response;
            }
            return "none";
        }
    }
}
