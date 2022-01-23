# MTCG
Monster Trading Card Game <br>

# POST <br>
Every request either needs username and password OR authentication token <br>
/login <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/register <br>
{ <br>
    "username": "string", <br>
    "password": "string" <br>
} <br>
/openPack <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/setDeck <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string", <br>
    "deck ": <int[]> <br>
} <br>
/startBattle <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/createTradeoffer <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "recipient_uid": <int>, <br>
    "i_receive": <int[]>, <br>
    "u_receive": <int[]> <br>
} <br>
/declineTradeoffer <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "tradeoffer_id": <int> <br>
} <br>
/acceptTradeoffer <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "tradeoffer_id": <int> <br>
} <br>
 
# GET <br>
Every request either needs username and password OR authentication token <br>
/getCollection <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/getTradeoffers <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/getUserprofile <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
    "access_token": "string" <br>
} <br>
/getAccessToken <br>
{ <br>
    "username": "string", <br>
    "password": "string", <br>
} <br>

