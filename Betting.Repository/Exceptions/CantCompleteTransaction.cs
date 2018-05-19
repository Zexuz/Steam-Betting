using System;

namespace Betting.Repository.Exceptions
{
    public class CantCompleteTransaction : Exception
    {
        public CantCompleteTransaction(Exception sqlException) : base("The transaction threw a sql error", sqlException)
        {
        }
    }
}