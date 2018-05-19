using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Betting.Models.Models;

namespace Betting.Repository.Services.Interfaces
{
    public interface ITransactionWrapper : IDisposable
    {
        Task<T>       ExecuteSqlCommand<T>(SqlQuery query);
        Task          ExecuteSqlCommand(SqlQuery query);
        void          Commit();
        void          Rollback();
        SqlTransaction Transaction { get; }
        SqlConnection SqlConnection { get; }
    }
}