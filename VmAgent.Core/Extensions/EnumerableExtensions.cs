// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T x in collection)
            {
                action(x);
            }
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> self)
        {
            return new ReadOnlyCollection<T>(self.ToList());
        }

        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                self.Add(item);
            }
        }
    }
}
