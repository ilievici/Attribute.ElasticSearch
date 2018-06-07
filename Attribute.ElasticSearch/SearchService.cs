using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Attribute.ElasticSearch.Attributes;
using Attribute.ElasticSearch.Domain;
using Nest;

namespace Attribute.ElasticSearch
{
    public interface ISearchService<T>
        where T : SearchDocument
    {
        SearchResult<T> Search(IElasticClientProvider elasticClientProvider, SearchFilter filter);
    }

    public abstract class BaseSearchService<T> : ISearchService<T>
        where T : SearchDocument
    {
        protected string TypeName => "items";
        protected List<PropertyInfoMeta> TypeMetadata;

        protected class PropertyInfoMeta
        {
            public PropertyInfoMeta(PropertyInfo info, ElasticMetaAttribute meta)
            {
                PropertyInfo = info;
                ElasticMeta = meta;
            }
            public PropertyInfo PropertyInfo { get; }
            public ElasticMetaAttribute ElasticMeta { get; }
        }

        public abstract SearchResult<T> Search(IElasticClientProvider elasticClientProvider, SearchFilter filter);
        protected abstract ISearchRequest BuildSearchRequest(SearchFilter filter);

        protected BaseSearchService()
        {
            TypeMetadata = GetTypeMetadata();
        }

        protected string GetIndexName(string clientId)
        {
            return clientId.Trim().ToLower();
        }
        protected SortOrder GetSortDirection(string direction)
        {
            var sortOrder = SortOrder.Descending;
            if (string.Compare(direction, "ASC", StringComparison.InvariantCulture) == 0)
            {
                sortOrder = SortOrder.Ascending;
            }

            return sortOrder;
        }
        protected virtual QueryContainer BuildCommonQuery(string searchType)
        {
            var container = new QueryContainer();

            container &= new QueryContainerDescriptor<T>()
                .QueryString(qs => qs
                    .Fields(f => f.Field(p => p.Type))
                    .Query(searchType)
                );

            return container;
        }

        private List<PropertyInfoMeta> GetTypeMetadata()
        {
            if (TypeMetadata == null)
            {
                TypeMetadata = new List<PropertyInfoMeta>();
            }

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo info in properties)
            {
                foreach (object attribute in info.GetCustomAttributes(true))
                {
                    ElasticMetaAttribute elasticMeta = attribute as ElasticMetaAttribute;
                    if (elasticMeta == null)
                    {
                        continue;
                    }

                    TypeMetadata.Add(new PropertyInfoMeta(info, elasticMeta));
                }
            }

            return TypeMetadata;
        }
    }

    public class SearchService<T> : BaseSearchService<T>
        where T : SearchDocument
    {
        private readonly IHashingService hashingService;

        public SearchService(IHashingService hashingService)
        {
            this.hashingService = hashingService;
        }

        public override SearchResult<T> Search(IElasticClientProvider elasticClientProvider, SearchFilter filter)
        {
            var searchRequest = BuildSearchRequest(filter);

            var elasticClient = elasticClientProvider.BuildElasticClient();
            var response = elasticClient.Search<T>(searchRequest);

            return new SearchResult<T>
            {
                Total = response.Total,
                TimeTook = response.Took,
                Result = response.Hits
                    .Select(s => s.Source)
                    .ToList()
            };
        }

        protected override ISearchRequest BuildSearchRequest(SearchFilter filter)
        {
            var indexName = GetIndexName(filter.ClientId);

            var commonQuery = BuildCommonQuery(filter.SearchType);
            var query = BuildQuery(filter.Criterias);
            var rangeQuery = BuildRangeQuery(filter.RangeCriterias);
            var misQuery = BuildMisQuery(filter.MisCriterias);
            var sort = BuildSort(filter.SortFields);

            var searchRequest = new SearchRequest<T>(Indices.Parse(indexName), Types.Parse(TypeName))
            {
                From = filter.ElasticStartingFromDoc,
                Size = filter.PageSize,
                Query = commonQuery && query && rangeQuery && misQuery,
                Sort = sort
            };

            return searchRequest;
        }

        private IList<ISort> BuildSort(Dictionary<string, string> sortFields)
        {
            var result = new List<ISort>();
            foreach (var criteria in sortFields.Where(s => !string.IsNullOrWhiteSpace(s.Value)))
            {
                var criteriaMeta = TypeMetadata.FirstOrDefault(s => string.Compare(s.ElasticMeta.Name, criteria.Key, StringComparison.InvariantCulture) == 0);
                if (criteriaMeta == null)
                {
                    throw new ParceException($"Unknown {criteria.Key} propery to sort by.");
                }

                SortOrder sortOrder = GetSortDirection(criteria.Value);

                if (criteriaMeta.ElasticMeta.IsHased)
                {
                    throw new ParceException("System limitation. Cannot perform sort by a hashed properly.");
                }

                var descriptor = new SortFieldDescriptor<T>()
                    .Order(sortOrder)
                    .Field(criteriaMeta.PropertyInfo);

                if (sortOrder == SortOrder.Ascending)
                {
                    descriptor = descriptor
                        .MissingFirst();
                }
                else
                {
                    descriptor = descriptor
                        .MissingLast();
                }

                result.Add(descriptor);
            }

            return result.Any() ? result : null;
        }

        private QueryContainer BuildQuery(Dictionary<string, string> criterias)
        {
            var container = new QueryContainer();

            foreach (var criteria in criterias)
            {
                var criteriaMeta = TypeMetadata.FirstOrDefault(s => string.Compare(s.ElasticMeta.Name, criteria.Key, StringComparison.InvariantCulture) == 0);
                if (criteriaMeta == null)
                {
                    throw new ParceException($"Unknown {criteria.Key} propery to search by.");
                }

                if (criteriaMeta.ElasticMeta.IsNested)
                {
                    continue;
                }

                var searchValue = criteria.Value;
                if (criteriaMeta.ElasticMeta.IsHased)
                {
                    searchValue = hashingService.GetMd5Hash(criteria.Value?.ToLower());
                }

                if (criteriaMeta.ElasticMeta.WildcardSearch)
                {
                    container &= new QueryContainerDescriptor<T>()
                        .Wildcard(wc => wc
                            .Field(criteriaMeta.PropertyInfo)
                            .Value($"*{searchValue}*")
                        );
                }
                else
                {
                    container &= new QueryContainerDescriptor<T>()
                        .QueryString(qs => qs
                            .Fields(criteriaMeta.PropertyInfo)
                            .Query(searchValue)
                        );
                }
            }

            return container;
        }

        private QueryContainer BuildRangeQuery(IList<SearchFilter.RangeFilter> rangeCriterias)
        {
            var container = new QueryContainer();

            foreach (var criteria in rangeCriterias)
            {
                var criteriaMeta = TypeMetadata.FirstOrDefault(s => string.Compare(s.ElasticMeta.Name, criteria.Name, StringComparison.InvariantCulture) == 0);
                if (criteriaMeta == null)
                {
                    throw new ParceException($"Unknown {criteria.Name} propery to search by.");
                }

                Action numberRangeAction = () =>
                {
                    try
                    {
                        var valueFrom = double.Parse(criteria.FromValue, CultureInfo.InvariantCulture);
                        var valueTo = double.Parse(criteria.ToValue, CultureInfo.InvariantCulture);

                        container &= new QueryContainerDescriptor<T>()
                            .Range(qs => qs.Field(criteriaMeta.PropertyInfo)
                            .GreaterThanOrEquals(valueFrom)
                            .LessThanOrEquals(valueTo));
                    }
                    catch (Exception e)
                    {
                        throw new ParceException($"Please input values by using dot separation. {e.Message}");
                    }
                };

                Action dateTimeRangeAction = () =>
                {
                    var dateFormat = "yyyy-MM-dd";
                    try
                    {
                        var dateFrom = DateTime.ParseExact(criteria.FromValue, dateFormat, CultureInfo.InvariantCulture);
                        var dateTo = DateTime.ParseExact(criteria.ToValue, dateFormat, CultureInfo.InvariantCulture);
                        var dateMathTimeUnit = DateMathTimeUnit.Day;

                        container &= new QueryContainerDescriptor<SearchDocument>()
                            .DateRange(qs => qs
                                .Field(criteriaMeta.PropertyInfo)
                                .GreaterThanOrEquals(DateMath.Anchored(dateFrom).RoundTo(dateMathTimeUnit))
                                .LessThanOrEquals(DateMath.Anchored(dateTo).RoundTo(dateMathTimeUnit))
                            );
                    }
                    catch (Exception e)
                    {
                        throw new ParceException($"Please input values by using '{dateFormat}' format. {e.Message}");
                    }
                };

                var @switch = new Dictionary<Type, Action>
                {
                    {typeof(DateTime), dateTimeRangeAction},
                    {typeof(DateTime?), dateTimeRangeAction},
                    {typeof(double), numberRangeAction},
                    {typeof(decimal), numberRangeAction},
                    {typeof(float), numberRangeAction},
                    {typeof(int), numberRangeAction},
                    {typeof(long), numberRangeAction}
                };

                var action = @switch
                    .Where(s => s.Key == criteriaMeta.PropertyInfo.PropertyType)
                    .Select(s => s.Value)
                    .FirstOrDefault();

                if (action == null)
                {
                    throw new ParceException($"No action defined for {criteriaMeta.PropertyInfo.PropertyType} type.");
                }

                action.Invoke();
            }

            return container;
        }

        private QueryContainer BuildMisQuery(IList<SearchFilter.MisFilter> misCriterias)
        {
            var container = new QueryContainer();

            if (!misCriterias.Any())
            {
                return container;
            }

            PropertyInfoMeta propertyInfoMeta = TypeMetadata.FirstOrDefault(s => string.Compare(s.ElasticMeta.Name, "MIS", StringComparison.InvariantCulture) == 0);
            if (propertyInfoMeta == null)
            {
                throw new ParceException("Unknown MIS propery to search by.");
            }

            if (!propertyInfoMeta.ElasticMeta.IsNested)
            {
                return container;
            }

            foreach (var criteria in misCriterias)
            {
                container &=
                    new QueryContainerDescriptor<T>()
                        .Nested(n => n
                            .Path(propertyInfoMeta.PropertyInfo)
                            .Query(q => q
                                .MultiMatch(mm => mm

                                    .Fields($"{propertyInfoMeta.PropertyInfo.Name}.{nameof(SearchDocument.MisFieldNestedTypes.Name)}")
                                    .Query(criteria.Name)
                                )
                            )
                        )
                    & new QueryContainerDescriptor<T>()
                        .Nested(n => n
                            .Path(propertyInfoMeta.PropertyInfo)
                            .Query(q => q
                                .Wildcard(mm => mm
                                    .Field($"{propertyInfoMeta.PropertyInfo.Name}.{nameof(SearchDocument.MisFieldNestedTypes.Value)}")
                                    .Value($"*{criteria.Value}*")
                                )
                            )
                        );
            }

            return container;
        }
    }
}