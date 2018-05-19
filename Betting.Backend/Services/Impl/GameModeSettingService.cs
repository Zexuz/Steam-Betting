using System;
using System.Threading.Tasks;
using Betting.Repository;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;

namespace Betting.Backend.Services.Impl
{
    public interface IGameModeSettingService
    {
        Task<object> Find(int id, GameModeType type);
        Task<object> GetSettingForType(GameModeType type);
    }

    public class GameModeSettingSettingService : IGameModeSettingService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;

        public GameModeSettingSettingService(IRepoServiceFactory repoServiceFactory)
        {
            _repoServiceFactory = repoServiceFactory;
        }

        public async Task<object> Find(int id, GameModeType type)
        {
            switch (type)
            {
                case GameModeType.JackpotCsgo:
                    return await _repoServiceFactory.JackpotSettingRepo.Find(id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public async Task<object> GetSettingForType(GameModeType type)
        {
            var gameMode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(type));

            switch (type)
            {
                case GameModeType.JackpotCsgo:
                    return await _repoServiceFactory.JackpotSettingRepo.Find(gameMode.CurrentSettingId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}