using System;
using System.IO;
using SQLite;

namespace Discord.Backend

{
    public class SqlLite
    {
        private string _databasePath;

        public SqlLite()
        {
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyData.db");
        }

        public void Start()
        {
            var db = GetNewConnection();
            CreateAllTables(db);
            
        }

        
        public void InsertUser(string discordId, string steamId)
        {
            var db = GetNewConnection();
            db.Insert(new User
            {
                DiscordId = discordId,
                SteamId = steamId,
                Created = DateTime.Now
            });
        }
        
        private SQLiteConnection GetNewConnection()
        {
            return new SQLiteConnection(_databasePath);
        }
        
        private void CreateAllTables(SQLiteConnection db)
        {
            db.CreateTable<User>();
        }
        
        private void DropAllTables(SQLiteConnection db)
        {
            db.DropTable<User>();
        }

        public bool DoesSteamIdExist(string steamId)
        {
            var db = GetNewConnection();
            var user = db.Table<User>().FirstOrDefault(u => u.SteamId == steamId);
            return user != null;
        }

        public ulong? FindDiscordIdFromSteamId(string steamId)
        {
            var db = GetNewConnection();
            try
            {
                return Convert.ToUInt64(db.Table<User>().FirstOrDefault(u => u.SteamId == steamId).DiscordId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string SteamId   { get; set; }
        [Unique]
        public string DiscordId { get; set; }
        public DateTime Created { get; set; }
    }
}