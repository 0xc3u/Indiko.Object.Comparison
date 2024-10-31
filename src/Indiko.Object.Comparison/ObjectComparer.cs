using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Indiko.Object.Comparison.Models;

namespace Indiko.Object.Comparison;

public static class ObjectComparer
{
    private static readonly Dictionary<(Type, string), Func<object, object>> _propertyGettersCache = new();

    /// <summary>
    /// Compares two objects and their properties to generate a comparison report.
    /// </summary>
    /// <param name="source">The source object for comparison.</param>
    /// <param name="destination">The destination object for comparison.</param>
    /// <param name="ignoreProperties">A list of property names to ignore during the comparison. Defaults to null.</param>
    /// <returns>A <see cref="ComparisonReport"/> that contains information about the equality and differences between the two objects.</returns>
    /// <exception cref="System.MethodAccessException">Thrown for protected methods on trusted objects in security critical contexts.</exception>
    /// <example>
    /// <code>
    /// var obj1 = new { Name = "John", Age = 30 };
    /// var obj2 = new { Name = "Doe", Age = 30 };
    /// var ignoredProps = new List&lt;string&gt;() { "Age" };
    /// var report = ObjectComparer.Compare(obj1, obj2, ignoredProps);
    /// </code>
    /// </example>
    /// <remarks>
    /// The method compares each property of the source object with the corresponding property of the destination object.
    /// Properties specified in the <paramref name="ignoreProperties"/> list are excluded from the comparison.
    /// The method also handles nullable types and checks for missing properties in either object.
    /// </remarks>
    public static ComparisonReport Compare(object source, object destination, List<string> ignoreProperties = null)
    {
        var report = new ComparisonReport { AreEqual = true };

        if (source == null && destination == null)
        {
            report.AreEqual = true;
            return report;
        }

        if (source == null || destination == null)
        {
            report.AreEqual = false;
            return report;
        }

        var sourceType = source.GetType();
        var destType = destination.GetType();

        var sourceProperties = sourceType.GetProperties()
            .Where(p => ignoreProperties?.Contains(p.Name) != true)
            .ToDictionary(p => p.Name);

        var destProperties = destType.GetProperties()
            .Where(p => ignoreProperties?.Contains(p.Name) != true)
            .ToDictionary(p => p.Name);

        foreach (var pair in sourceProperties)
        {
            if (destProperties.TryGetValue(pair.Key, out var destProperty))
            {
                var sourceValue = GetPropertyValue(source, sourceType, pair.Key);
                var destValue = GetPropertyValue(destination, destType, pair.Key);

                var sourceTypeToCompare = Nullable.GetUnderlyingType(pair.Value.PropertyType) ?? pair.Value.PropertyType;
                var destTypeToCompare = Nullable.GetUnderlyingType(destProperty.PropertyType) ?? destProperty.PropertyType;

                if (sourceTypeToCompare != destTypeToCompare || !Equals(sourceValue, destValue))
                {
                    report.AreEqual = false;
                    report.Differences.Add(new Difference
                    {
                        PropertyName = pair.Key,
                        SourceValue = sourceValue,
                        DestinationValue = destValue,
                        SourceType = pair.Value.PropertyType,
                        DestinationType = destProperty.PropertyType
                    });
                }

                destProperties.Remove(pair.Key);
            }
            else
            {
                // Property is present in source but missing in destination
                report.AreEqual = false;
                report.Differences.Add(new Difference
                {
                    PropertyName = pair.Key,
                    SourceValue = GetPropertyValue(source, sourceType, pair.Key),
                    DestinationValue = null,
                    SourceType = pair.Value.PropertyType,
                    DestinationType = null
                });
            }
        }

        // Add differences for properties that are present in destination but missing in source
        foreach (var pair in destProperties)
        {
            report.AreEqual = false;
            report.Differences.Add(new Difference
            {
                PropertyName = pair.Key,
                SourceValue = null,
                DestinationValue = GetPropertyValue(destination, destType, pair.Key),
                SourceType = null,
                DestinationType = pair.Value.PropertyType
            });
        }

        return report;
    }

    public static ComparisonReport CompareLists<T>(IList<T> sourceList, IList<T> destinationList, List<string> ignoreProperties = null)
    {
        var report = new ComparisonReport { AreEqual = true };

        if (sourceList == null && destinationList == null)
        {
            report.AreEqual = true;
            return report;
        }

        if (sourceList == null || destinationList == null || sourceList.Count != destinationList.Count)
        {
            report.AreEqual = false;
            report.Differences.Add(new Difference
            {
                PropertyName = "ListCount",
                SourceValue = sourceList?.Count,
                DestinationValue = destinationList?.Count,
                SourceType = typeof(int),
                DestinationType = typeof(int)
            });
            return report;
        }

        for (int i = 0; i < sourceList.Count; i++)
        {
            var itemReport = Compare(sourceList[i], destinationList[i], ignoreProperties);
            if (!itemReport.AreEqual)
            {
                report.AreEqual = false;
                report.Differences.AddRange(itemReport.Differences.Select(d =>
                {
                    d.PropertyName = $"Item[{i}].{d.PropertyName}";
                    return d;
                }));
            }
        }

        return report;
    }


    private static object GetPropertyValue(object obj, Type type, string propertyName)
    {
        var key = (type, propertyName);

        if (!_propertyGettersCache.TryGetValue(key, out var getter))
        {
            var parameter = Expression.Parameter(typeof(object), "obj");
            var castedObj = Expression.Convert(parameter, type);
            var property = Expression.Property(castedObj, propertyName);
            var convert = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(convert, parameter);

            getter = lambda.Compile();
            _propertyGettersCache[key] = getter;
        }

        return getter(obj);
    }
}
