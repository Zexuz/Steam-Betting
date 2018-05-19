using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IItemDescriptionQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.ItemDescription> itemDescriptions);
        SqlQuery InsertReturnsId(DatabaseModel.ItemDescription itemDescription);
        SqlQuery Insert(DatabaseModel.ItemDescription itemDescription);
        SqlQuery GetAll();
        SqlQuery GetFromIds(List<int> ids);
        SqlQuery GetFromId(int id);
        SqlQuery GetFromName(string name);
        SqlQuery GetFromNames(List<string> names);
        SqlQuery Update(DatabaseModel.ItemDescription itemDescription);
        SqlQuery UpdateImgUrl(string name, string imgUrl);
    }
}