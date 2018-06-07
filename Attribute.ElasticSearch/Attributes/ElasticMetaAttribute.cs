
namespace Attribute.ElasticSearch.Attributes
{
    public class ElasticMetaAttribute : System.Attribute
    {
        public string Name { get; set; }
        public bool WildcardSearch { get; set; }
        public bool IsHased { get; set; }
        public bool IsNested { get; set; }

        public ElasticMetaAttribute(string name)
        {
            Name = name;
        }
    }
}