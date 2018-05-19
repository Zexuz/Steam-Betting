using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;

namespace Betting.Backend.Services.Impl
{
    public class SettingsService : ISettingsService
    {
        private readonly IRepoServiceFactory _repoServiceFactory;

        private DatabaseModel.Settings _settings;

        public SettingsService(IRepoServiceFactory repoServiceFactory)
        {
            _repoServiceFactory = repoServiceFactory;
        }


        public async Task<DatabaseModel.Settings> GetCurrentSettings()
        {
            if (_settings == null)
            {
                _settings = await _repoServiceFactory.SettingRepoService.GetSettingsAsync();
            }

            return _settings;
        }

        public async Task SetOrUpdateSettings(DatabaseModel.Settings settings)
        {
            await _repoServiceFactory.SettingRepoService.SetSettingsAsync(settings);
            _settings = settings;
        }

        public DatabaseModel.Settings CreateSettingsObject()
        {
            var settings = GetEmptySettingsObject();

            Console.WriteLine("No settings table was found, Creating one!");
            foreach (var prop in typeof(DatabaseModel.Settings).GetProperties().ToList())
            {
                Console.Write($"Need value for {prop.Name} : ");
                var value      = Console.ReadLine();
                var changeType = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(settings, changeType);
            }

            return settings;
        }

        public DatabaseModel.Settings GetEmptySettingsObject()
        {
            return (DatabaseModel.Settings) FormatterServices.GetUninitializedObject(typeof(DatabaseModel.Settings));
        }
    }
}