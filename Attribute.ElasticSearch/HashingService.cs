namespace Attribute.ElasticSearch
{
    public interface IHashingService
    {
        string GetMd5Hash(string input);
    }

    public class HashingService : IHashingService
    {
        public string GetMd5Hash(string input)
        {
            return input;
        }
    }
}