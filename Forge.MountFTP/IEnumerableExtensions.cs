using System;
using System.Collections.Generic;
using System.Linq;

namespace Forge.MountFTP
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            enumerable
                .ToList()
                .ForEach(li => action(li));
        }
    }
}