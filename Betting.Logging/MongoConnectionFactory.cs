using MongoDB.Driver;

namespace CsgoDraffle.Logging
{
    public class MongoConnectionFactory
    {
        public MongoClient GetMongoCleint(string url)
        {
            return new MongoClient(url);
        }
    }
}