using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGTest.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable, Func<T,object> func)
        {
            return enumerable.WhereNotNull().Where(x => func(x) != null);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach(var elem in enumerable)
            {
                action(elem);
            }
        }
    }
}
