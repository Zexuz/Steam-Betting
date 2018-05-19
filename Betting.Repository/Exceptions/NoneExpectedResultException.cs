namespace Betting.Repository.Exceptions
{
    public class NoneExpectedResultException : System.Exception
    {
        public NoneExpectedResultException(string e) : base(e)
        {
        }
    }
}