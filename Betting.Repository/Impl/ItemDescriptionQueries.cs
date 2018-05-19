using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Helpers;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class ItemDescriptionQueries : IItemDescriptionQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.ItemDescription> itemDescriptions)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < itemDescriptions.Count; index++)
            {
                listOfSqlValues.Add($"(@name{index} ,@value{index}, @appId{index}, @contextId{index}, @imageUrl{index},@valid{index})");
                dict.Add($"@name{index}", ItemDescriptionHelper.ToDatabase(itemDescriptions[index].Name));
                dict.Add($"@value{index}", itemDescriptions[index].Value);
                dict.Add($"@appId{index}", itemDescriptions[index].AppId);
                dict.Add($"@contextId{index}", itemDescriptions[index].ContextId);
                dict.Add($"@imageUrl{index}", itemDescriptions[index].ImageUrl);
                dict.Add($"@valid{index}", itemDescriptions[index].Valid);
            }

            return new SqlQuery(
                $"INSERT INTO [ItemDescription] (Name, Value, AppId, ContextId, ImageUrl, Valid) VALUES {string.Join(",", listOfSqlValues)};", dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.ItemDescription itemDescription)
        {
            var dict = new Dictionary<string, object>
            {
                {"@name", ItemDescriptionHelper.ToDatabase(itemDescription.Name)},
                {"@value", itemDescription.Value},
                {"@appId", itemDescription.AppId},
                {"@imgUrl", itemDescription.ImageUrl},
                {"@contextId", itemDescription.ContextId},
                {"@valid", itemDescription.Valid}
            };

            return new SqlQuery(
                "INSERT INTO [ItemDescription] (Name, Value, AppId, ContextId, ImageUrl, Valid) OUTPUT INSERTED.Id VALUES (@name,@value,@appId,@contextId,@imgUrl,@valid);",
                dict);
        }

        public SqlQuery Insert(DatabaseModel.ItemDescription itemDescription)
        {
            return InsertRange(new List<DatabaseModel.ItemDescription> {itemDescription});
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [ItemDescription]", null);
        }

        public SqlQuery GetFromIds(List<int> ids)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [ItemDescription] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [ItemDescription] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetFromName(string name)
        {
            return new SqlQuery("SELECT * FROM [ItemDescription] WHERE Name =@name",
                new Dictionary<string, object> {{"@name", ItemDescriptionHelper.ToDatabase(name)}});
        }

        public SqlQuery GetFromNames(List<string> names)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < names.Count; index++)
            {
                strArr.Add($"@name{index}");
                dict.Add($"@name{index}", ItemDescriptionHelper.ToDatabase(names[index]));
            }

            var str = string.Join(",", strArr);

            return new SqlQuery($"SELECT * FROM [ItemDescription] WHERE Name IN ({str})", dict);
        }

        public SqlQuery Update(DatabaseModel.ItemDescription itemDescription)
        {
            var dict = new Dictionary<string, object>
            {
                {"@value", itemDescription.Value},
                {"@name", ItemDescriptionHelper.ToDatabase(itemDescription.Name)},
            };
            return new SqlQuery($"UPDATE [ItemDescription] SET Value = @value, Valid = 1 WHERE Name = @name", dict);
        }

        public SqlQuery UpdateImgUrl(string name, string imgUrl)
        {
            var dict = new Dictionary<string, object>
            {
                {"@imgUrl", imgUrl},
                {"@name", ItemDescriptionHelper.ToDatabase(name)},
            };
            return new SqlQuery($"UPDATE [ItemDescription] SET ImageUrl = @imgUrl WHERE Name = @name", dict);
        }
    }
}