using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using Bettingv1.Models;
using Bettingv1.Models.SqlMapping;
using Dapper;
using MongoDB.Driver;

namespace Bettingv1.Migration
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=BettingMigration;Trusted_Connection=True;");
            connection.Open();
            return connection;
        }


        private void Start()
        {
            MapDapper<User>();
            MapDapper<Match>();
            MapDapper<ItemOnMatch>();


            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("mydb");
            var collection = database.GetCollection<MatchModel>("Bettingv1History");

            var connection = GetOpenConnection();
            var matches = connection.Query<Match>("SELECT * FROM Betting_backup.[Match] WHERE [value] IS NOT NULL AND [status] = 'done'").ToList();

            for (var index = 0; index < matches.Count; index++)
            {
                var match = matches[index];

                var percentage = Math.Round((((double) index) / (double) matches.Count), 4);
                Console.WriteLine($"{percentage               * 100}%");

                var winner = connection.Query<UserModel>("SELECT id AS SteamId, avatar AS ImageUrl,name FROM Betting_backup.[users] WHERE id = @id",
                    new {Id = match.WinnerSteamId}).Single();

                var itemOnMatches = connection.Query<ItemOnMatch>(
                    "Select * FROM BettingMigration.Betting_backup.weaponsindatabase WHERE matchid = @id",
                    new {Id = match.RoundId}).ToList();

                if (itemOnMatches.Count == 0) continue;

                var uniqueSteamIdOnMatch = itemOnMatches.Select(u => u.OwnerSteamId).Distinct();

                var bets = new List<UserBet>();

                var usersOnMatch = uniqueSteamIdOnMatch
                    .Select(userSteamId =>
                        connection
                            .Query<User>("Select * FROM BettingMigration.Betting_backup.users WHERE id = @id", new {Id = userSteamId})
                            .Single())
                    .ToList();

                foreach (var user in usersOnMatch)
                {
                    var bet = new UserBet {User = user};
                    var usersItems = itemOnMatches.Where(item => item.OwnerSteamId == user.SteamId).ToList();

                    foreach (var itemOnMatch in usersItems)
                    {
                        var item = new ItemModel();
                        item.ImageUrl = itemOnMatch.ImageUrl;
                        item.Name = itemOnMatch.Name;
                        item.Value = StringToDecimalMapper.ToDecimal(itemOnMatch.Value);
                        bet.Items.Add(item);
                    }

                    bets.Add(bet);
                }

                var matchModel = new MatchModel
                {
                    Created = match.Created,
                    RoundId = match.RoundId,
                    Winner = winner,
                    Bets = bets
                };

                collection.InsertOne(matchModel);
            }

            connection.Close();
        }

        private void MapDapper<TModel>()
        {
            SqlMapper.SetTypeMap(
                typeof(TModel),
                new CustomPropertyTypeMap(
                    typeof(TModel),
                    (type, columnName) =>
                        type.GetProperties().FirstOrDefault(prop =>
                            prop.GetCustomAttributes(false)
                                .OfType<ColumnAttribute>()
                                .Any(attr => attr.Name == columnName))));
        }
    }
}