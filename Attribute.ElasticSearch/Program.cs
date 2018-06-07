using System;
using System.Collections.Generic;
using Attribute.ElasticSearch.Domain;
using Newtonsoft.Json;

namespace Attribute.ElasticSearch
{
    class Program
    {
        static readonly IHashingService hashingService = new HashingService();
        static readonly IElasticClientProvider elasticClientProvider = new ElasticClientProvider("http://localhost:9200/");

        static void Main()
        {
            var filter = new SearchFilter("autopay", "cutexas_autopay")
            {
                Criterias = new Dictionary<string, string>
                {
                    {"LAST_NAME", "SMITH"},
                    //{"KKT","KKT"}
                },
                SortFields = new Dictionary<string, string>
                {
                    {"ENTERED_DATE", "DESC"}
                },
                RangeCriterias = new List<SearchFilter.RangeFilter>
                {
                    new SearchFilter.RangeFilter("AMOUNT", "1", "99"),
                    new SearchFilter.RangeFilter("ENTERED_DATE", DateTime.UtcNow.ToString("yyyy-MM-dd"), DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd"))
                }
            };

            //PaymentSearchDocument
            //AutopaySearchDocument
            //PbtSearchDocument

            var documents = Search<PaymentSearchDocument>(filter);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(documents, Formatting.Indented));
            Console.ReadKey();
        }

        private static SearchResult<T> Search<T>(SearchFilter filter)
            where T : SearchDocument
        {
            var service = new SearchService<T>(hashingService);
            return service.Search(elasticClientProvider, filter);
        }
    }
}
