namespace Betting.Backend.Managers.Interface
{
    public interface IHotStatusManager
    {
        void AddHotMatch(string steamId, string roundId);
        void RemoveExperiedHotStatuses();
    }
}