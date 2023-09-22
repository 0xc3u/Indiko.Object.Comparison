using FluentAssertions;

namespace Indiko.Object.Comparison.Tests;

public class ObjectComparerTests
{
    [Fact]
    public void Compare_ShouldReturnTrue_WhenObjectsAreEqual()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "John", Age = 25 };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeTrue();
        result.Differences.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ShouldReturnFalse_WhenObjectsAreNotEqual()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "Jane", Age = 30 };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Should().NotBeEmpty();
    }

    [Fact]
    public void Compare_ShouldIgnoreSpecifiedProperties()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "Jane", Age = 25 };
        var ignoreProperties = new List<string> { "Name" };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2, ignoreProperties);

        // Assert
        result.AreEqual.Should().BeTrue();
        result.Differences.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ShouldReturnFalse_WhenOneObjectIsNull()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        
        // Act
        var result = ObjectComparer.Compare(obj1, null);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ShouldReturnFalse_WhenPropertyMissingInDestination()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "John" };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Should().NotBeEmpty();
        result.Differences.Should().ContainSingle(d => d.PropertyName == "Age");
    }

}