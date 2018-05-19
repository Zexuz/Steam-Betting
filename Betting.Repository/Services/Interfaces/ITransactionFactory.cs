namespace Betting.Repository.Services.Interfaces
{
    public interface ITransactionFactory
    {
        ITransactionWrapper BeginTransaction();
    }
}