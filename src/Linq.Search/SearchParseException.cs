using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CityofEdmonton.Linq.Search
{
    [Serializable]
    public class SearchParseException : Exception
    {
        public SearchParseException()
        {
        }

        public SearchParseException(string message) : base(message)
        {
        }

        public SearchParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SearchParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
