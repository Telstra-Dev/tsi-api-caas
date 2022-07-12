using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Telstra.Common
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> source, IEnumerable<T> newList)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(newList, nameof(newList));

            if (source is List<T> concreteList)
            {
                concreteList.AddRange(newList);
            }
            else
            {
                foreach (var element in newList)
                {
                    source.Add(element);
                }
            }
        }

        public static Dictionary<TKey, TElement> ToSafeDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentException("source");
            if (keySelector == null)
                throw new ArgumentException("keySelector");
            if (elementSelector == null)
                throw new ArgumentException("elementSelector");

            var d = new Dictionary<TKey, TElement>(comparer);
            foreach (var element in source)
            {
                if (!d.ContainsKey(keySelector(element)))
                    d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        public static Dictionary<TKey, TSource> ToSafeDictionary<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {

            if (source == null)
                throw new ArgumentException("source");
            if (keySelector == null)
                throw new ArgumentException("keySelector");

            var d = new Dictionary<TKey, TSource>();
            foreach (var element in source)
            {
                if (!d.ContainsKey(keySelector(element)))
                    d.Add(keySelector(element), element);
            }

            return d;
        }

        public static string Join(this IEnumerable<string> data, string separater)
        {
            return string.Join(separater, data);
        }

        public static string Join(this IEnumerable<int> data, string separater)
        {
            return string.Join(separater, data);
        }

        public static void ForEachItem<T>(this List<T> source, Action<T, int> predicate)
        {
            int count = 0;
            source.ForEach(m =>
            {
                predicate.Invoke(m, count);
                count++;
            });
        }

        public static void ForEachItem<T>(this IEnumerable<T> source, Action<T, int> predicate)
        {
            ForEachItem(source.ToList(), predicate);
        }

        public static List<T> AddItem<T>(this List<T> @this, T item)
        {
            @this.Add(item);
            return @this;
        }

        public static IEnumerable<T> AddItem<T>(this IEnumerable<T> @this, T item)
        {
            return @this.ToList().AddItem(item);
        }

        public static T[] AddItem<T>(this T[] @this, T item)
        {
            return @this.ToList().AddItem(item).ToArray();
        }

        public static JObject ToJson(this Dictionary<string, string> data)
        {
            return JObject.Parse(data.Serialize());
        }

        public static JObject ToJson<T>(this Dictionary<string, T> data)
        {
            return JObject.Parse(data.Serialize());
        }
    }
}
