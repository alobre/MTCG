using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            if(access_token != null)
            {
                var cmd = new Npgsql.NpgsqlCommand("", conn);
                bool isValid = await db.ValidateToken(access_token, cmd);
                if (isValid == true)
                {
                    return "{\"msg\": \"access_token valid!\", \"success\": true}";
                }
                else
                {
                    return "{\"msg\": \"access_token not valid! Please Login again!\", \"success\": false}";
                }
            } else if(username != null && password != null )
            {
                string response = await db.LoginByCredentials(username, password, conn);
                return response;
            }
            return "none";
        }
        public static async Task<string> OpenPack(string username = "", string password = "", string access_token = "")
        {
            if(username != null && password !=null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if(json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);
                    //reduce coins by 5
                    var newCoins = await db.OpenPack(json.uid, cmd);
                    //get random card id's (cid)
                    var cardIdArray = await db.GetRandom(cmd);
                    //assign cards to user
                    db.AssignCardsToUser(json.uid, cardIdArray, cmd);
                    //get CardList
                    List<Card> CardList = await db.CreateCardList(cardIdArray, cmd);
                    //string response = (Card)JsonSerializer.Deserialize<Card>(new Card()); // HIER

                    return $"{{\"msg\": \"Pack purchase successful!\", \"success\": true, \"coins\": {newCoins}}}";
                }
            }
            return "{\"msg\": \"Pack purchase failed!\", \"success\": false}";
            
        }

    }
}
