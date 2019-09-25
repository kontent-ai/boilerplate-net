using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kentico.Kontent.Boilerplate.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Adds a range to a list, if the range is not null.
        /// </summary>
        /// <typeparam name="T">List entry type</typeparam>
        /// <param name="list">List to add to</param>
        /// <param name="rangeToAdd">Range to add</param>
        public static void AddNonNullRange<T>(this List<T> list, IEnumerable<T> rangeToAdd)
        {
            if (rangeToAdd != null)
            {
                list.AddRange(rangeToAdd);
            }
        }
    }
}