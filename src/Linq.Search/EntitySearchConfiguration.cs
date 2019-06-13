using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CityofEdmonton.Linq.Search
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntitySearchConfiguration<T>
    {
        internal EntitySearchConfiguration(SearchConfigurationOptions options)
        {
            _options = options;
            _typeMappings = new ConcurrentDictionary<string, LambdaExpression>();
            _typePredicateMappings = new ConcurrentDictionary<string, Expression<Func<T, string, bool>>>();
            _defaultSearchFields = new ConcurrentBag<LambdaExpression>();
        }

        private readonly SearchConfigurationOptions _options;
        internal SearchConfigurationOptions Options => _options;

        private readonly ConcurrentDictionary<string, LambdaExpression> _typeMappings;
        internal LambdaExpression GetTypeMapping(string searchField)
        {
            if (string.IsNullOrWhiteSpace(searchField))
            {
                throw new ArgumentNullException(nameof(searchField));
            }
            EnsureInitialized();

            var key = searchField.ToUpperInvariant();
            if (_typeMappings.TryGetValue(key, out var value))
            {
                return value;

            }

            return null;
        }

        private bool isInitialized = false;
        internal void EnsureInitialized()
        {
            if (!(isInitialized || _defaultSearchFields.Any() || _typeMappings.Any() || _typePredicateMappings.Any()))
            {
                try
                {
                    ApplyDefaultConfigToEntity();
                }
                finally
                {
                    isInitialized = true;
                }
            }
        }


        private readonly ConcurrentDictionary<string, Expression<Func<T, string, bool>>> _typePredicateMappings;
        internal Expression<Func<T, string, bool>> GetPredicate(string searchField)
        {
            if (string.IsNullOrWhiteSpace(searchField))
            {
                throw new ArgumentNullException(nameof(searchField));
            }
            EnsureInitialized();

            var key = searchField.ToUpperInvariant();
            if (_typePredicateMappings.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }



        private readonly ConcurrentBag<LambdaExpression> _defaultSearchFields;
        internal IEnumerable<LambdaExpression> DefaultSearchFields
        {
            get
            {
                EnsureInitialized();
                return _defaultSearchFields;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntitySearchConfiguration<T> AddDefaultSearchField<TProperty>(Expression<Func<T, TProperty>> property)
        {
            _defaultSearchFields.Add(property);
            return this;
        }
        public EntitySearchConfiguration<T> Map(string property)
        {
            return Map(property, property);
        }
        public EntitySearchConfiguration<T> Map(string property, string alias)
        {
            var entityType = typeof(T);
            var propertyInfo = entityType.GetProperty(property);
            if (propertyInfo == null)
            {
                throw new SearchConfigurationException($"Unable to find a property with name { property } in type { entityType.FullName }");
            }

            var p = Expression.Parameter(entityType);
            var lambda = Expression.Lambda(Expression.Property(p, property), p);

            // we don't know the type of TProperty at compile time so we have to call it this way
            return typeof(EntitySearchConfiguration<T>).GetMethods()
                .Where(x => x.Name == "Map" && x.GetGenericArguments().Length == 1)
                .Select(x => new { Method = x, Parameters = x.GetParameters() })
                .Where(x => x.Parameters.Count() == 2 && x.Parameters[1].ParameterType == typeof(string))
                .Single()
                .Method
                .MakeGenericMethod(propertyInfo.PropertyType)
                .Invoke(this, new object[] { lambda, alias }) as EntitySearchConfiguration<T>;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntitySearchConfiguration<T> Map<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            throw new NotSupportedException("This overload is currently not supported. " + 
                "In the future we should be able to infer the name from the lambda, but for now " +
                "please specify the name using one of the other overloads");

            //typeMappings.Add(property.Name, property);
            //return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public EntitySearchConfiguration<T> Map<TProperty>(Expression<Func<T, TProperty>> property, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentNullException(nameof(alias));
            }
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            _typeMappings.AddOrUpdate(alias.ToUpperInvariant(), property, (k, v) => property);
            return this;
        }

        // /// <summary>
        // ///     Maps a property to a specific search query
        // /// </summary>
        // /// <typeparam name="P"> The objet type to be queried. </typeparam>
        // /// <param name="predicate">The property to map as search field to. </param>
        // /// <param name="alias">Names to refer to the search field. </param>
        // /// <returns>This object, for chaining. </returns>
        public EntitySearchConfiguration<T> Map<TProperty>(Expression<Func<T, TProperty>> property, params string[] alias)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            foreach (var a in alias.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                _typeMappings.AddOrUpdate(a.ToUpperInvariant(), property, (k, v) => property);
            }
            return this;
        }


        /// <summary>
        ///     Maps a property to a specific search query
        /// </summary>
        /// <param name="predicate">The property to map as search field to. </param>
        /// <param name="alias">Name to refer to the search field. </param>
        /// <returns>This object, for chaining. </returns>
        public EntitySearchConfiguration<T> Map(Expression<Func<T, string, bool>> predicate, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentNullException(nameof(alias));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            _typePredicateMappings.AddOrUpdate(alias.ToUpperInvariant(), predicate, (k, v) => predicate);
            return this;
        }

        /// <summary>
        ///     Maps a property to a specific search query
        /// </summary>
        /// <param name="predicate">The property to map as search field to. </param>
        /// <param name="alias">Names to refer to the search field. </param>
        /// <returns>This object, for chaining. </returns>
        public EntitySearchConfiguration<T> Map(Expression<Func<T, string, bool>> predicate, params string[] alias)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (var a in alias.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                _typePredicateMappings.AddOrUpdate(a.ToUpperInvariant(), predicate, (k,v) => predicate);
            }

            return this;
        }


        internal void ApplyDefaultConfigToEntity()
        {
            // we will set defaultSearchFields to all public string in the type,
            // and all other public fields will be searchable by name
            var configType = typeof(EntitySearchConfiguration<T>);
            var entityType = typeof(T);
            var entityProperties = entityType.GetProperties();
            var defaultSearchFieldNamesUpper = Options.DefaultSearchFieldNames.Select(s => s?.ToUpperInvariant()).ToArray();
            var defaultSearchFieldPropertyCount = entityProperties
                .Count(p => defaultSearchFieldNamesUpper.Contains(p.Name.ToUpperInvariant()));

            foreach (var p in entityProperties)
            {
                var paramExpr = Expression.Parameter(entityType);
                var propExpr = Expression.Property(paramExpr, p.Name);
                Expression lambda = Expression.Lambda(propExpr, paramExpr);

                configType.GetMethods()
                    .Where(m => m.Name == "Map"
                        && m.IsGenericMethod
                        && m.GetParameters().Count() == 2
                        && m.GetParameters().ElementAt(1).ParameterType == typeof(string))
                    .Single()
                    .MakeGenericMethod(p.PropertyType)
                    .Invoke(this, new object[] { lambda, p.Name });

                // add the property if it is a string and the property name is Title, Name or Description
                // -OR- if there are no properties called Title, Name or Description, we add all string properties
                if (p.PropertyType == typeof(string) &&
                    (defaultSearchFieldPropertyCount == 0 ||
                        defaultSearchFieldNamesUpper.Contains(p.Name.ToUpperInvariant())))
                {

                    configType.GetMethod("AddDefaultSearchField")
                        .MakeGenericMethod(p.PropertyType)
                        .Invoke(this, new object[] { lambda });
                }
            }

            // if we didn't actually add any

        }

    }
}
