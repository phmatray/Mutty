# Analyzers

Mutty includes Roslyn analyzers to help you use the library correctly and avoid common mistakes.

| ID | Meaning | Severity |
|---|---|---|
| [MUTTY001](#mutty001-mutablegeneration-attribute-can-only-be-applied-to-records) | Applied to a non-record (class, struct, interface, enum). | Error |
| [MUTTY002](#mutty002-generic-records-are-not-supported) | Applied to a generic record. | Error |
| [MUTTY003](#mutty003-nested-records-are-not-supported) | Applied to a record nested inside another type. | Error |

## MUTTY001: MutableGeneration attribute can only be applied to records

### Description

The `[MutableGeneration]` attribute is designed to work only with record types. Applying it to classes, structs, interfaces, enums, or other non-record types will not generate the expected mutable wrapper and will produce this analyzer error.

### Severity

Error

### Example

#### ❌ Incorrect Usage

```C#
using Mutty;

// Error MUTTY001: The [MutableGeneration] attribute 
// can only be applied to record types
[MutableGeneration]
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Error MUTTY001: The [MutableGeneration] attribute 
// can only be applied to record types
[MutableGeneration]
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

// Error MUTTY001: The [MutableGeneration] attribute 
// can only be applied to record types
[MutableGeneration]
public interface IPerson
{
    string Name { get; }
    int Age { get; }
}

// Error MUTTY001: The [MutableGeneration] attribute
// can only be applied to record types
[MutableGeneration]
public enum Status
{
    Active,
    Inactive
}
```

#### ✅ Correct Usage

```C#
using Mutty;

// Valid: Applied to a record
[MutableGeneration]
public record Person(string Name, int Age);

// Valid: Applied to a record class (explicit syntax)
[MutableGeneration]
public record class Employee(string Name, string Department);

// Valid: Applied to a record struct
[MutableGeneration]
public record struct Point(int X, int Y);

// Valid: Applied to a record with additional members
[MutableGeneration]
public record Product(string Name, decimal Price)
{
    public string Category { get; init; } = "General";
}
```

### How to Fix

To fix this error, you have several options:

1. **Convert to a record**: If the type is meant to be immutable, convert it to a record type:
   ```C#
   // Before
   [MutableGeneration]
   public class Person
   {
       public string Name { get; set; }
       public int Age { get; set; }
   }
   
   // After
   [MutableGeneration]
   public record Person(string Name, int Age);
   ```

2. **Remove the attribute**: If the type should remain a class/struct/interface, remove the `[MutableGeneration]` attribute:
   ```C#
   // Before
   [MutableGeneration]
   public class PersonService { }
   
   // After
   public class PersonService { }
   ```

3. **Create a separate record**: If you need both a mutable class and an immutable record, create them separately:
   ```C#
   // Immutable record with mutable wrapper generation
   [MutableGeneration]
   public record PersonData(string Name, int Age);
   
   // Separate mutable class for other purposes
   public class PersonEntity
   {
       public string Name { get; set; }
       public int Age { get; set; }
   }
   ```

### Why This Matters

The Mutty library is specifically designed to generate mutable wrappers for immutable record types. Records in C# are designed to be immutable by default, which makes them perfect candidates for scenarios where you need both immutable and mutable representations of the same data structure.

Applying the `[MutableGeneration]` attribute to non-record types would not work because:
- The source generator expects specific record syntax and semantics
- The generated code relies on record features like positional parameters and init-only properties
- The library's design patterns are optimized for record types

### Suppression

This analyzer should not be suppressed as it indicates a fundamental misuse of the library. If you believe you have a valid use case that triggers this analyzer incorrectly, please file an issue on the Mutty GitHub repository.

## MUTTY002: Generic records are not supported

### Description

Mutty cannot generate a mutable wrapper for an open generic record (e.g. `record Box<T>`): the generated class, constructor, and conversion operators would be malformed. When `[MutableGeneration]` is applied to a generic record, the generator skips it and the analyzer reports this error.

### Severity

Error

### Example

```C#
using Mutty;

// Error MUTTY002: generic records are not supported
[MutableGeneration]
public record Box<T>(T Value);
```

### How to Fix

Either remove the type parameter(s) and use a concrete record, or remove the `[MutableGeneration]` attribute:

```C#
// Concrete record — supported
[MutableGeneration]
public record IntBox(int Value);
```

## MUTTY003: Nested records are not supported

### Description

Mutty emits the mutable wrapper at namespace scope, so it cannot wrap a record declared *inside* another type. When `[MutableGeneration]` is applied to a nested record, the generator skips it and the analyzer reports this error.

### Severity

Error

### Example

```C#
using Mutty;

public class Outer
{
    // Error MUTTY003: records nested in another type are not supported
    [MutableGeneration]
    public record Item(int Value);
}
```

### How to Fix

Move the record to namespace (top-level) scope:

```C#
[MutableGeneration]
public record Item(int Value);

public class Outer
{
    // ...
}
```