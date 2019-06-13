using System;
using System.Runtime.Serialization;

namespace CityofEdmonton.Linq.Search
{
    [Serializable]
    internal class SearchConfigurationException : Exception
    {
        public SearchConfigurationException()
        {
        }

        public SearchConfigurationException(string message) : base(message)
        {
        }

        public SearchConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SearchConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}