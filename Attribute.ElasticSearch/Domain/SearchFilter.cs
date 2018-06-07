using System;
using System.Collections.Generic;

namespace Attribute.ElasticSearch.Domain
{
    /// <summary>
    /// Base search filter
    /// </summary>
    public class SearchFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class.
        /// </summary>
        /// <param name="searchType">Search type</param>
        /// <param name="page">Page value</param>
        /// <param name="clientId">Client ID</param>
        /// <param name="pageSize">Page size</param>
        public SearchFilter(string searchType, string clientId, int pageSize = 10, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(searchType))
            {
                throw new ArgumentNullException(nameof(searchType));
            }

            SearchType = searchType;
            ClientId = clientId;
            Page = page <= 0
                ? 1
                : page;
            PageSize = pageSize;

            Criterias = new Dictionary<string, string>();
            SortFields = new Dictionary<string, string>();
        }

        /// <summary>
        /// Search type
        /// </summary>
        public string SearchType { get; }

        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Page
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets the adjusted value of FROM index for search request
        /// </summary>
        public int ElasticStartingFromDoc
        {
            get
            {
                var from = Page == 1
                    ? 0
                    : (Page - 1) * PageSize;

                return from;
            }
        }

        /// <summary>
        /// Specific fields search, when it is known
        /// </summary>
        public List<RangeFilter> RangeCriterias { get; set; }

        /// <summary>
        /// Specific fields search, when it is known
        /// </summary>
        public Dictionary<string, string> Criterias { get; set; }

        /// <summary>
        /// Sort fields
        /// </summary>
        public Dictionary<string, string> SortFields { get; set; }

        public class RangeFilter
        {
            public RangeFilter(string name, string fromValue, string toValue)
            {
                Name = name;
                FromValue = fromValue;
                ToValue = toValue;
            }

            public string Name { get; }
            public string FromValue { get; }
            public string ToValue { get; }
        }
    }
}