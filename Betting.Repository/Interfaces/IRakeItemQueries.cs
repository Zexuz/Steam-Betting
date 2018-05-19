using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IRakeItemQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.RakeItem> items);
        SqlQuery InsertReturnsId(DatabaseModel.RakeItem rakeItem);
        SqlQuery Insert(DatabaseModel.RakeItem rakeItem);
        SqlQuery GetAll();
        SqlQuery GetFromId(int id);
        SqlQuery GetItemsFromId(List<int> ids);
        SqlQuery GetFromAssetId(AssetAndDescriptionId info);
        SqlQuery GetFromMatchId(int matchId);
        SqlQuery GetItemsFromAssetId(List<AssetAndDescriptionId> info);
        SqlQuery SetAsSold(List<string> assetAndDescriptionIds);
        SqlQuery GetAllWithSoldStatus(bool status);
    }
}