using FluentAssertions;
using Indiko.Object.Comparison.Extensions;

namespace Indiko.Object.Comparison.Tests;

public class ObjectComparerTests
{
    public enum StatusEnum
    {
        Active,
        Inactive
    }

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

    [Fact]
    public void Compare_ShouldReturnTrue_WhenNullableTypesAreEqual()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = (int?)25 };
        var obj2 = new { Name = "John", Age = (int?)25 };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeTrue();
        result.Differences.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ShouldReturnFalse_WhenNullableTypesAreNotEqual()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = (int?)25 };
        var obj2 = new { Name = "John", Age = (int?)30 };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Should().NotBeEmpty();
        result.Differences.Should().ContainSingle(d => d.PropertyName == "Age");
    }

    [Fact]
    public void Compare_ShouldReturnFalse_WhenOneNullableTypeIsNull()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = (int?)25 };
        var obj2 = new { Name = "John", Age = (int?)null };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Should().NotBeEmpty();
        result.Differences.Should().ContainSingle(d => d.PropertyName == "Age");
    }

    [Fact]
    public void Compare_ShouldHaveCorrectNumberOfDifferences()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25, City = "New York" };
        var obj2 = new { Name = "Jane", Age = 30, Country = "USA" };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Count.Should().Be(4); // 'Name', 'Age', 'City', 'Country'

        result.Differences.Should().Contain(d => d.PropertyName == "Name");
        result.Differences.Should().Contain(d => d.PropertyName == "Age");
        result.Differences.Should().Contain(d => d.PropertyName == "City");
        result.Differences.Should().Contain(d => d.PropertyName == "Country");
    }

    [Fact]
    public void Compare_ShouldReturnTrue_WhenAllPropertiesIgnored()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "Jane", Age = 30 };
        List<string> ignoreProperties = new List<string> { "Name", "Age" };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2, ignoreProperties);

        // Assert
        result.AreEqual.Should().BeTrue();
        result.Differences.Should().BeEmpty();
    }

    [Fact]
    public void Compare_ShouldHandleMixedPropertyTypes()
    {
        // Arrange
        var obj1 = new
        {
            Name = "John",
            Age = 25,
            Scores = new List<int> { 90, 85 },
            Status = StatusEnum.Active
        };

        var obj2 = new
        {
            Name = "Jane",
            Age = 30,
            Scores = new List<int> { 70, 65 },
            Status = StatusEnum.Inactive
        };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Count.Should().Be(4); // Name, Age, Scores, Status
        result.Differences.Should().Contain(d => d.PropertyName == "Name");
        result.Differences.Should().Contain(d => d.PropertyName == "Age");
        result.Differences.Should().Contain(d => d.PropertyName == "Scores");
        result.Differences.Should().Contain(d => d.PropertyName == "Status");
    }

    [Fact]
    public void Compare_ShouldHandleNestedObjects()
    {
        // Arrange
        var address1 = new { Street = "Main St", ZipCode = "12345" };
        var address2 = new { Street = "Elm St", ZipCode = "67890" };
        var obj1 = new { Name = "John", Age = 25, Address = address1 };
        var obj2 = new { Name = "Jane", Age = 30, Address = address2 };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Count.Should().Be(3); // Name, Age, Address
        result.Differences.Should().Contain(d => d.PropertyName == "Name");
        result.Differences.Should().Contain(d => d.PropertyName == "Age");
        result.Differences.Should().Contain(d => d.PropertyName == "Address");
    }

    [Fact]
    public void Compare_ShouldNotAlterInputObjects()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "Jane", Age = 30 };

        var obj1Clone = new { Name = obj1.Name, Age = obj1.Age };
        var obj2Clone = new { Name = obj2.Name, Age = obj2.Age };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        // Check that the objects are not altered
        obj1.Name.Should().Be(obj1Clone.Name);
        obj1.Age.Should().Be(obj1Clone.Age);
        obj2.Name.Should().Be(obj2Clone.Name);
        obj2.Age.Should().Be(obj2Clone.Age);
    }

    [Fact]
    public void Compare_ShouldCorrectlyCountDifferences()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25, IsActive = true };
        var obj2 = new { Name = "Jane", Age = 30, IsActive = false };

        // Act
        var result = ObjectComparer.Compare(obj1, obj2);

        // Assert
        result.AreEqual.Should().BeFalse();
        result.Differences.Count.Should().Be(3); // Name, Age, IsActive

        // Verify each difference
        var expectedDifferences = new List<string> { "Name", "Age", "IsActive" };
        foreach (var diff in result.Differences)
        {
            expectedDifferences.Should().Contain(diff.PropertyName);
        }

        // Validate that count matches the length of expected differences
        result.Differences.Count.Should().Be(expectedDifferences.Count);
    }

    [Fact]
    public void Compare_ShouldHandleNullCases()
    {
        // Arrange & Act
        var result1 = ObjectComparer.Compare(null, null);
        var result2 = ObjectComparer.Compare(new { Name = "John", Age = 25 }, null);
        var result3 = ObjectComparer.Compare(null, new { Name = "Jane", Age = 30 });

        // Assert
        // Both objects are null, should be equal
        result1.AreEqual.Should().BeTrue();
        result1.Differences.Should().BeEmpty();

        // One object is null, should not be equal
        result2.AreEqual.Should().BeFalse();

        // One object is null, should not be equal
        result3.AreEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_ShouldHandleNullableTypes()
    {
        // Arrange
        int? age1 = 25;
        int? age2 = null;
        int? age3 = 30;
        var obj1 = new { Name = "John", Age = age1 };
        var obj2 = new { Name = "Jane", Age = age2 };
        var obj3 = new { Name = "John", Age = age1 };
        var obj4 = new { Name = "Jane", Age = age3 };

        // Act
        var result1 = ObjectComparer.Compare(obj1, obj2); // Different: One has null Age
        var result2 = ObjectComparer.Compare(obj1, obj3); // Equal: Both have same Age
        var result3 = ObjectComparer.Compare(obj2, obj4); // Different: One has null Age

        // Assert
        // Different due to null Age
        result1.AreEqual.Should().BeFalse();
        result1.Differences.Should().Contain(d => d.PropertyName == "Age");

        // Equal as both ages are 25
        result2.AreEqual.Should().BeTrue();
        result2.Differences.Should().BeEmpty();

        // Different due to null Age
        result3.AreEqual.Should().BeFalse();
        result3.Differences.Should().Contain(d => d.PropertyName == "Age");
    }

    [Fact]
    public void Compare_ShouldIgnoreNonExistentProperties()
    {
        // Arrange
        var obj1 = new { Name = "John", Age = 25 };
        var obj2 = new { Name = "Jane", Age = 25 };
        List<string> ignoreProperties = new() { "Country", "Email" }; // Properties not on obj1 or obj2

        // Act
        var result = ObjectComparer.Compare(obj1, obj2, ignoreProperties);

        // Assert
        // Should be considered different because only Age is the same, Name is different.
        result.AreEqual.Should().BeFalse();

        // Make sure "Country" and "Email" are not causing issues.
        result.Differences.Should().NotContain(d => d.PropertyName == "Country" || d.PropertyName == "Email");
    }


}