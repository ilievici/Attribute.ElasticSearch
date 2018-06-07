using System;
using System.Text;
using Elasticsearch.Net;
using Nest;

namespace Attribute.ElasticSearch
{
    public interface IElasticClientProvider
    {
        IElasticClient BuildElasticClient(int requestTimeout = 10);
    }

    public class ElasticClientProvider : IElasticClientProvider
    {
        private readonly string connectionString;

        public ElasticClientProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IElasticClient BuildElasticClient(int requestTimeout = 10)
        {
            var node = new UriBuilder(connectionString);

            var connectionPool = new SingleNodeConnectionPool(node.Uri);
            var settings = new ConnectionSettings(connectionPool);

            //Ensures a full log from ES engine
            settings.EnableDebugMode();
            settings.IncludeServerStackTraceOnError();
            settings.PrettyJson();
            settings.MaximumRetries(2);
            settings.RequestTimeout(TimeSpan.FromSeconds(requestTimeout));
            settings.ThrowExceptions(false);

            settings.OnRequestCompleted(response =>
            {
                if (response.RequestBodyInBytes != null)
                {
                    var query = Encoding.UTF8.GetString(response.RequestBodyInBytes);
                    Console.WriteLine(query);
                }
            });

            return new ElasticClient(settings);
        }
    }
}