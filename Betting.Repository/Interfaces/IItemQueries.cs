using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Interfaces
{
    public interface IItemQueries
    {
        SqlQuery InsertRange(List<DatabaseModel.Item> items);
        SqlQuery InsertReturnsId(DatabaseModel.Item item);
        SqlQuery Insert(DatabaseModel.Item item);
        SqlQuery GetAll();
        SqlQuery GetFromId(int id);
        SqlQuery GetItemsFromId(List<int> ids);
        SqlQuery ChangeOwner(int itemId, int newUserId);
        SqlQuery GetFromAssetId(AssetAndDescriptionId info);
        SqlQuery GetItemsFromAssetId(List<AssetAndDescriptionId> info);
        SqlQuery GetItemsThatUserOwns(int userId);
        SqlQuery DeleteRange(List<AssetAndDescriptionId> items);
        SqlQuery DeleteSingle(AssetAndDescriptionId item);
        SqlQuery ChangeOwner(List<int> itemIds, int newUserId);
        SqlQuery DeleteRange(List<int> rakeResItemIdsToWinner);
        SqlQuery ChangeOwner(List<AssetAndDescriptionId> rakeResItemIdsToWinner, int newUserId);
    }
}