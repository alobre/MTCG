using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MTCG
{
    class Program
    {
        static List<User> Users_Online = new List<User>();
        static TcpListener listener;

        public object Show { get; private set; }

        static void Main(string[] args)
        {
            Server().Wait();
        }

        public static async Task Server()
        {
            int port = 5555;
            if (listener == null) listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine("Start");
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Factory.StartNew(async (state) =>
                {
                    TcpClient client1 = (TcpClient)state;
                    using (client1)
                    {
                        bool ende = true;
                        while (ende)
                        {
                            using (var ns = client1.GetStream())
                            {
                                string Firstline, body = "";
                                byte[] msg = new byte[4096];
                                ns.Read(msg, 0, msg.Length);
                                string request = Encoding.UTF8.GetString(msg).TrimEnd('\0');
                                string requestType, query;
                                string jsonString = "";
                                credentials jsonBody;
                                Body requestBody;
                                using (StringReader reader = new StringReader(request))
                                {

                                    string line = await reader.ReadLineAsync();
                                    Console.WriteLine(line);
                                    Firstline = line;
                                    bool insideBody = false;
                                    requestType = Firstline.Split(' ')[0];
                                    query = Firstline.Split(' ')[1];
                                    while (line != null)
                                    {
                                        line = await reader.ReadLineAsync();
                                        Console.WriteLine(line);
                                        if (line == "{" || insideBody)
                                        {
                                            insideBody = true;
                                            jsonString += line;
                                            if (line == "}") insideBody = false;
                                        }
                                    }

                                    body = await reader.ReadLineAsync();
                                    Console.WriteLine(body);
                                    Console.WriteLine(jsonString);
                                    jsonBody = JsonSerializer.Deserialize<credentials>(jsonString);
                                    requestBody = JsonSerializer.Deserialize<Body>(jsonString);
                                    Console.WriteLine(jsonBody.access_token);
                                }

                                switch (requestType.ToLower())
                                {
                                    case "post":
                                        string response = await POST(query, jsonBody, requestBody.deck);
                                        Console.WriteLine("Response: " + response);
                                        string res = 
                                        @$"HTTP/1.1 200 OK
                                        Last-Modified: 25.20.2021
                                        Content-Length:
                                        Content-Type: application/json
                                        Connection: Closed

                                        {response}";
                                        byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(res);
                                        ns.Write(bytesToSend, 0, bytesToSend.Length);
                                        break;
                                    case "get":
                                        string GETresponse = await GET(query, jsonBody);
                                        Console.WriteLine("Response: " + GETresponse);
                                        string GETres =
                                        @$"HTTP/1.1 200 OK
                                        Last-Modified: 25.20.2021
                                        Content-Length:
                                        Content-Type: application/json
                                        Connection: Closed

                                        {GETresponse}";
                                        byte[] bytesToSend_GET = System.Text.Encoding.ASCII.GetBytes(GETres);
                                        ns.Write(bytesToSend_GET, 0, bytesToSend_GET.Length);
                                        break;
                                }

                            }
                        }
                        client1.Close();
                    }

                }, client);

            }
        }

        static public async Task<string> POST(string query, credentials body, int[] deck = null)
        {
            string response = "string.Empty";
            switch (query)
            {
                case "/login":
                    response = await DatabaseHandler.Login(username: body.username, password: body.password, access_token: body.access_token);
                    break;
                case "/register":
                    DatabaseHandler.Register(body.username, body.password);
                    break;
                case "/openPack":
                    response = await DatabaseHandler.OpenPack(username: body.username, password: body.password, access_token: body.access_token);
                    break;
                case "/setDeck":
                    response = await DatabaseHandler.SetDeck(deck, username: body.username, password: body.password, access_token: body.access_token);
                    break;
                case "/startBattle":
                    response = await DatabaseHandler.StartBattle(username: body.username, password: body.password, access_token: body.access_token);
                    break;
            }
            //query != "/startBattle") 
            return response;
        }
        static public async Task<string> GET(string query, credentials body)
        {
            string response = "string.Empty";
            switch (query)
            {
                case "/getCollection":
                    response = await DatabaseHandler.ShowCollection(username: body.username, password: body.password, access_token: body.access_token);
                    break;
            }
            return response;
        }

        /*static public void Register(string username = "", string password = "", string access_token = "")
        {
            try
            {
                User neuer_User = new User(username, password);
                User.Alle_User.Add(neuer_User);
            }
            catch (Exception e)
            {
                throw new Exception("Registrierung fehlerhaft");
            }

        }*/

        /*static public void Login(string username = ", string password)
        {
            try
            {
                Login(username, password);
            }
            catch (Exception e)
            {
                throw new Exception("Registrierung fehlerhaft");
            }
        }*/

        public void acquire_cards()
        {

        }
    }
}