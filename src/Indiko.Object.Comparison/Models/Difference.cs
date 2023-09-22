using System;

namespace Indiko.Object.Comparison.Models;
public class Difference
{
    public string PropertyName { get; set; }
    public object SourceValue { get; set; }
    public object DestinationValue { get; set; }
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }
}