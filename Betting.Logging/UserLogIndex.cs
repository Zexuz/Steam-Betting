using System.Collections.Generic;
using MongoDB.Bson;

namespace CsgoDraffle.Logging
{
    public class UserLogIndex
    {
        public ObjectId Id      { get; set; }
        public string   SteamId { get; set; }

        public List<Log> Logs { get; set; }
    }
}