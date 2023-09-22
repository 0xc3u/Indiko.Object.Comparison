# Indiko.Object.Comparison

## Overview

This library is designed for comparing two objects of the same or different types. The main functionality is encapsulated in the following four classes:

- `ObjectExtensions`: Provides a static method that extends `object` type for comparison.
- `ComparisonReport`: Holds the result of the comparison between two objects.
- `Difference`: Details the difference in a particular property between two objects.
- `ObjectComparer`: Handles the core logic of comparing objects.

## ObjectExtensions

This static class contains a single extension method `CompareTo`, which takes two objects, and an optional list of properties to ignore during comparison. The method returns a `ComparisonReport`.

### Usage

```csharp
var report = obj1.CompareTo(obj2);
```

or

```csharp
var report = obj1.CompareTo(obj2, new List<string>{"PropertyToIgnore"});
```

## ComparisonReport

A class that contains the result of a comparison operation. It has two properties:

- `AreEqual`: A boolean indicating if the objects are equal.
- `Differences`: A list of `Difference` objects indicating which properties differ between the two objects.

## Difference

Represents a single difference between the compared objects. Holds information about:

- `PropertyName`: The property that differs.
- `SourceValue`: The value of the property in the source object.
- `DestinationValue`: The value of the property in the destination object.
- `SourceType`: The type of the property in the source object.
- `DestinationType`: The type of the property in the destination object.

## ObjectComparer

This static class does the heavy lifting of object comparison. It has a static method `Compare` that takes two generic objects and an optional list of properties to ignore.

The method returns a `ComparisonReport` that can be further examined to understand how the two objects differ.

### Core Logic

1. Initialize a `ComparisonReport` object, setting `AreEqual` to `true`.
2. Compare each property of the source object with the destination object.
3. If a property is only present in one object or the values are different, update `ComparisonReport` accordingly.

### Usage

This is mainly used internally by `ObjectExtensions`, but can also be used directly.

```csharp
var report = ObjectComparer.Compare(obj1, obj2);
```

or

```csharp
var report = ObjectComparer.Compare(obj1, obj2, new List<string>{"PropertyToIgnore"});
```
