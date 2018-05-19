using System;
using MongoDB.Bson;

namespace CsgoDraffle.Logging
{
    public class ErrorLogIndex
    {
        public ObjectId Id          { get; set; }
        public string   Message     { get; set; }
        public string   StackTrace  { get; set; }
        public string   ErrorPath   { get; set; }
        public DateTime Time        { get; set; }
        public string   UserSteamId { get; set; }
    }
}