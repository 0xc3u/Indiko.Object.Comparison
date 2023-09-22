using Indiko.Object.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Indiko.Object.Comparison;

public static class ObjectComparer
{
    public static ComparisonReport Compare<TType>(TType source, TType destination, List<string> ignoreProperties = null)
    {
        return Compare(source, destination, ignoreProperties);
    }

    public static ComparisonReport Compare(object source, object destination, List<string> ignoreProperties = null)
    {
        var report = new ComparisonReport { AreEqual = true };

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
