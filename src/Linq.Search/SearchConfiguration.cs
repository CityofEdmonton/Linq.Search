using System;
using System.Collections.Generic;
using System.Text;
using CityofEdmonton.Linq.Search;

namespace Linq.Search
{
    public class SearchConfiguration
    {
        public static void ConfigureSearch(Action<SearchConfigurationOptions> options = null)
        {
            _currentConfig = new SearchConfigurationOptions();
            options?.Invoke(_currentConfig);
        }

        private static SearchConfigurationOptions _currentConfig;
        internal static SearchConfigurationOptions CurrentConfig
        {
            get
            {
                // ensure we never return null
                if (_currentConfig == null)
                {
                    _currentConfig = new SearchConfigurationOptions();
                }
                return _currentConfig;
            }
        }
    }
}
