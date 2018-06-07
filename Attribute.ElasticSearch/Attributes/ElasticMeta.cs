
namespace Attribute.ElasticSearch.Attributes
{
    public class ElasticMeta : System.Attribute
    {
        public string Name { get; set; }
        public bool WildcardSearch { get; set; }
        public bool IsHased { get; set; }

        public ElasticMeta(string name, bool wildcardSearch, bool isHased = false)
        {
            Name = name;
            WildcardSearch = wildcardSearch;
            IsHased = isHased;
        }
    }
}