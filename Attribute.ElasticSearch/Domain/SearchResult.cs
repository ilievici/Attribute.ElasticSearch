using System.Collections.Generic;

namespace Attribute.ElasticSearch.Domain
{
    public class SearchResult<T>
    {
        public long Total { get; set; }
        public IEnumerable<T> Result { get; set; }
        public long TimeTook { get; set; }
    }
}