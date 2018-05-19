using System;
using Betting.Models.Models;

namespace Betting.Backend.Extension
{
    public static class JackpotSetting
    {
        public static JackpotMatchSetting ToJackpotMatchSetting(this DatabaseModel.JackpotSetting jackpotSetting)
        {
            return new JackpotMatchSetting
            (
                jackpotSetting.Rake,
                TimeSpan.FromMilliseconds(jackpotSetting.TimmerInMilliSec),
                jackpotSetting.ItemsLimit,
                jackpotSetting.MaxItemAUserCanBet,
                jackpotSetting.MinItemAUserCanBet,
                jackpotSetting.MaxValueAUserCanBet,
                jackpotSetting.MinValueAUserCanBet,
                jackpotSetting.AllowCsgo,
                jackpotSetting.AllowPubg,
                TimeSpan.FromMilliseconds(jackpotSetting.DraftingTimeInMilliSec),
                jackpotSetting.DraftingGraph
            );
        }
    }
}