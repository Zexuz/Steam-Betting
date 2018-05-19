namespace Betting.Repository.Exceptions
{
    public class CanNotBetOnClosedMatchException : System.Exception
    {
        public CanNotBetOnClosedMatchException(string s) : base(s)
        {
        }
    }
}