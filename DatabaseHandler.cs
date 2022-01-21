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
        public static async Task<string> Register(string username, string password)
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            db.Register(username, password, conn);
            //conn.Close();
            return "{\"msg\": \"Login successful!\", \"success\": true}";
        }
        public static async Task<string> Login(string username = "", string password = "", string access_token = "")
        {
            Database db = new Database();
            Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
            if (access_token != null)
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
            }
            else if (username != null && password != null)
            {
                string response = await db.LoginByCredentials(username, password, conn);
                return response;
            }
            return "none";
        }
        public static async Task<string> OpenPack(string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
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
                    string response = JsonSerializer.Serialize<List<Card>>(CardList);

                    return $"{{\"msg\": \"Pack purchase successful!\", \"success\": true, \"coins\": {newCoins}, \"data\": {response}}}";
                }
            }
            return "{\"msg\": \"Pack purchase failed!\", \"success\": false}";
        }
        public static async Task<string> ShowCollection(string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);

                    //get CardList
                    List<Card> CardList = await db.GetUsersCollection(json.uid, cmd);

                    string response = JsonSerializer.Serialize<List<Card>>(CardList);

                    return $"{{\"msg\": \"Successfully retrieved collection!\", \"success\": true, \"data\": {response}}}";
                }
            }
            return "{\"msg\": \"Collection call error\", \"success\": false}";
        }
        public static async Task<string> SetDeck(int[] deck, string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);

                    UserProfile user = new UserProfile();
                    user.deck = deck;
                    int[] confirmedDeck = await db.SetDeck(json.uid, user, cmd);

                    return $"{{\"msg\": \"Successfully Updated Deck\", \"success\": true, \"data\": {confirmedDeck.ToString()}}}";
                }
            }
            return $"{{\"msg\": \"Updating deck failed make sure you own the cards and don't have any duplicates.\", \"success\": false, \"deck\": []}}";
        }
        public static async Task<string> StartBattle(string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);

                    //STEP 1: GET USER PROFILE
                    UserProfile user = await db.GetUserProfile(json.uid, cmd);

                    //STEP 2: QUEUE
                    db.StartBattle(user, cmd);


                    return $"{{\"msg\": \"Searching for Battle\", \"success\": true}}";
                }
            }
            return "{\"msg\": \"Searching for Battle failed\", \"success\": false}";
        }
        public static async Task<string> Tradeoffer(int recipient_uid, int[] i_receive, int[] u_receive,  string action, int tradeoffer_id = -1, string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);
                    switch (action)
                    {
                        case "accept":
                            string accept_response = await db.AcceptTradeoffer(json.uid, tradeoffer_id, cmd);
                            return accept_response;
                        case "decline":
                            string decline_response = await db.DeclineTradeoffer(json.uid, tradeoffer_id, cmd);
                            return decline_response;
                        case "create":
                            string create_response = db.CreateTradeoffer(json.uid, recipient_uid, i_receive, u_receive, cmd);
                            return create_response;
                    }
                    return "";
                }
                return "";
            }
            return "";

        }
        public static async Task<string> GetTradeoffers(string username = "", string password = "", string access_token = "")
        {
            if (username != null && password != null || access_token != null)
            {
                string loginResponse = await Login(username, password, access_token);
                responseJson json = JsonSerializer.Deserialize<responseJson>(loginResponse);
                if (json.success == true)
                {
                    Database db = new Database();
                    Npgsql.NpgsqlConnection conn = await db.ConnectDB("localhost", "postgres", "postgres", "mtcg");
                    var cmd = new Npgsql.NpgsqlCommand("", conn);
                    var tradeoffers = db.GetTradeoffers(json.uid, cmd);
                    
                    string response = JsonSerializer.Serialize<List<Tradeoffer>>(tradeoffers.AllTradeoffers);
                    return $"{{\"msg\": \"Successfully retrieved tradeoffers!\", \"success\": true, \"data\": {response}}}";
                }
                return $"{{\"msg\": \"Authentication failed\", \"success\": false}}";
            }
            return $"{{\"msg\": \"Authentication failed\", \"success\": false}}";
        }
    }
}
