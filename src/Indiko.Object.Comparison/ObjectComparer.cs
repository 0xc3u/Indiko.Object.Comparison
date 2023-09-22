using Indiko.Object.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Indiko.Object.Comparison;

public static class ObjectComparer
{
    /// <summary>
    /// Compares two objects and their properties to generate a comparison report.
    /// </summary>
    /// <param name="source">The source object for comparison.</param>
    /// <param name="destination">The destination object for comparison.</param>
    /// <param name="ignoreProperties">A list of property names to ignore during the comparison. Defaults to null.</param>
    /// <returns>A <see cref="ComparisonReport"/> that contains information about the equality and differences between the two objects.</returns>
    /// <exception cref="System.Reflection.TargetException">Thrown if the property is non-static and the object is null.</exception>
    /// <exception cref="System.MethodAccessException">Thrown for protected methods on trusted objects in security critical contexts.</exception>
    /// <exception cref="System.Reflection.TargetInvocationException">Thrown if the initial set method on a property throws an exception.</exception>
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

        var sourceProperties = source.GetType().GetProperties()
            .Where(p => ignoreProperties?.Contains(p.Name) != true)
            .ToDictionary(p => p.Name);

        var destProperties = destination.GetType().GetProperties()
            .Where(p => ignoreProperties?.Contains(p.Name) != true)
            .ToDictionary(p => p.Name);

        foreach (var pair in sourceProperties)
        {
            if (destProperties.TryGetValue(pair.Key, out var destProperty))
            {
                var sourceValue = pair.Value.GetValue(source);
                var destValue = destProperty.GetValue(destination);

                var sourceNullableType = Nullable.GetUnderlyingType(pair.Value.PropertyType);
                var destNullableType = Nullable.GetUnderlyingType(destProperty.PropertyType);

                var sourceTypeToCompare = sourceNullableType ?? pair.Value.PropertyType;
                var destTypeToCompare = destNullableType ?? destProperty.PropertyType;

                if (sourceTypeToCompare != destTypeToCompare || !object.Equals(sourceValue, destValue))
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
                // present in source, missing in destination
                report.AreEqual = false;
                report.Differences.Add(new Difference
                {
                    PropertyName = pair.Key,
                    SourceValue = pair.Value.GetValue(source),
                    DestinationValue = null,
                    SourceType = pair.Value.PropertyType,
                    DestinationType = null
                });
            }
        }

        foreach (var pair in destProperties)
        {
            // present in destination, missing in source
            report.AreEqual = false;
            report.Differences.Add(new Difference
            {
                PropertyName = pair.Key,
                SourceValue = null,
                DestinationValue = pair.Value.GetValue(destination),
                SourceType = null,
                DestinationType = pair.Value.PropertyType
            });
        }

        return report;
    }
}
