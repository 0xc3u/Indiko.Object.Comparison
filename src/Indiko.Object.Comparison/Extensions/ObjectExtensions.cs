using Indiko.Object.Comparison.Models;
using System.Collections.Generic;

namespace Indiko.Object.Comparison.Extensions;
public static class ObjectExtensions
{
    public static ComparisonReport CompareTo(this object source, object destination, List<string> ignoreProperties = null)
    {
        return ObjectComparer.Compare(source, destination, ignoreProperties);
    }
}
