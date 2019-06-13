using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CityofEdmonton.Linq.Search
{
    /// <summary>
    /// 
    /// </summary>
    public class SearchConfigurationOptions
    {
        private readonly ConcurrentDictionary<Type, object> _entityConfigurations = new ConcurrentDictionary<Type, object>();

        //private static SearchConfigurationOptions _current = null;

        //internal static SearchConfigurationOptions Current
        //{
        //    get
        //    {
        //        if (_current == null)
        //        {
        //            _current = new SearchConfigurationOptions();
        //        }
        //        return _current;
        //    }
        //}

        private readonly ICollection<string> _defaultSearchFieldNames = new List<string>();
        internal IEnumerable<string> DefaultSearchFieldNames => _defaultSearchFieldNames.Any()
            ? _defaultSearchFieldNames
            : new string[] { "Name", "Title", "Description" };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntitySearchConfiguration<T> Entity<T>()
        {
            var config = (EntitySearchConfiguration<T>)
                _entityConfigurations.GetOrAdd(typeof(T), x => new EntitySearchConfiguration<T>(this));

            return config;
        }
    }
}
