using System;

namespace Attribute.ElasticSearch
{
    [Serializable]
    public class ParceException : Exception
    {
        public ParceException(string message)
            : base(message)
        {
        }
    }
}