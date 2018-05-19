using System;
using System.Collections.Generic;
using Betting.Models.Models;
using Newtonsoft.Json;

namespace Betting.WebApi.Helpers
{
    public static class SteamPlayerMapping
    {
        public static DatabaseModel.User GetUserFromSteamObject(string json)
        {
            var obj = JsonConvert.DeserializeObject<RootObject>(json);

            var user = obj.response.players[0];
            return new DatabaseModel.User(user.steamid, user.personaname, user.avatar, null, DateTime.Now, DateTime.Now,false,"Go to your profile to change this quote to something more personal.");
        }
        
        internal static Player GetSteamUserFromSteamObject(string json)
        {
            var obj = JsonConvert.DeserializeObject<RootObject>(json);

            var user = obj.response.players[0];
            return user;
        }

        internal class Player
        {
            public string steamid                  { get; set; }
            public int    communityvisibilitystate { get; set; }
            public int    profilestate             { get; set; }
            public string personaname              { get; set; }
            public int    lastlogoff               { get; set; }
            public string profileurl               { get; set; }
            public string avatar                   { get; set; }
            public string avatarmedium             { get; set; }
            public string avatarfull               { get; set; }
            public int    personastate             { get; set; }
            public string primaryclanid            { get; set; }
            public int    timecreated              { get; set; }
            public int    personastateflags        { get; set; }
        }

        private class Response
        {
            public List<Player> players { get; set; }
        }

        private class RootObject
        {
            public Response response { get; set; }
        }
    }
}