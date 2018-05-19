namespace Steam.Market.Models
{
    public class LoginError
    {
        public ErrorType Type          { get; set; }
        public bool      CaptchaNeeded { get; set; }
        public long      CaptchaGid    { get; set; }
        public string    Message       { get; set; }
    }
}