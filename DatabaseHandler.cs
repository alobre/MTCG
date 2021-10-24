using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    class DatabaseHandler
    {
        public static async void Login(string username = "", string password = "", string access_token = "")
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            if(access_token != "")
            {
                
            } else if(username != "" && password != "" )
            {
                Console.WriteLine(await db.LoginByCredentials(username, password, conn)); 
            }
            
        }
    }
}
