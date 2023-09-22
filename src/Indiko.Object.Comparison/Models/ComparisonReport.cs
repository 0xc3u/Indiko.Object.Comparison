using System.Collections.Generic;

namespace Indiko.Object.Comparison.Models;
public class ComparisonReport
{
    public bool AreEqual { get; set; }
    public List<Difference> Differences { get; set; } = new List<Difference>();
}
