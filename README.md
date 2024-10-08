# Mutty

Immutable Record Mutation Made Easy

| ![Logo Mutty](https://raw.githubusercontent.com/phmatray/Mutty/main/logo.png) | Mutty is a C# Incremental Source Generator that provides a convenient way to work with immutable records by generating mutable wrappers for them. These wrappers allow you to modify properties of immutable records in a clean, controlled manner and then convert them back into immutable records. |
|-------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

[![phmatray - Mutty](https://img.shields.io/static/v1?label=phmatray&message=Mutty&color=blue&logo=github)](https://github.com/phmatray/Mutty "Go to GitHub repo")
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![stars - Mutty](https://img.shields.io/github/stars/phmatray/Mutty?style=social)](https://github.com/phmatray/Mutty)
[![forks - Mutty](https://img.shields.io/github/forks/phmatray/Mutty?style=social)](https://github.com/phmatray/Mutty)

[![GitHub tag](https://img.shields.io/github/tag/phmatray/Mutty?include_prereleases=&sort=semver&color=blue)](https://github.com/phmatray/Mutty/releases/)
[![issues - Mutty](https://img.shields.io/github/issues/phmatray/Mutty)](https://github.com/phmatray/Mutty/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/phmatray/Mutty)](https://github.com/phmatray/Mutty/pulls)
[![GitHub contributors](https://img.shields.io/github/contributors/phmatray/Mutty)](https://github.com/phmatray/Mutty/graphs/contributors)
[![GitHub last commit](https://img.shields.io/github/last-commit/phmatray/Mutty)](https://github.com/phmatray/Mutty/commits/master)

---

## 📝 Table of Contents

<!-- TOC -->
* [Mutty](#mutty)
  * [📝 Table of Contents](#-table-of-contents)
  * [📌 Features](#-features)
    * [Current Features](#current-features)
  * [How Mutty Works](#how-mutty-works)
    * [Example Usage](#example-usage)
    * [How to Use the Generated Code](#how-to-use-the-generated-code)
      * [Deep Nesting Example](#deep-nesting-example)
      * [Comparison with `with` Notation](#comparison-with-with-notation)
    * [Ideal for Flux Architecture](#ideal-for-flux-architecture)
    * [Installation](#installation)
    * [Best Practices](#best-practices)
    * [Contributing](#contributing)
    * [License](#license)
<!-- TOC -->

---

## 📌 Features

### Current Features

- [x] **Automated Mutable Wrappers**: Automatically generates mutable wrapper classes for your immutable records using Roslyn's Incremental Source Generation.
- [x] **Deep Nesting Support**: Easily handle complex nested structures without tedious and error-prone manual code.
- [x] **Immutable to Mutable Conversion**: Seamlessly switch between immutable and mutable versions of your records using implicit conversions.
- [x] **Ideal for Flux Architecture**: Works great with Flux architecture, allowing you to manage state changes in a predictable and immutable way.
- [x] **Helper Methods**:
    - [x] Provides a `Produce` method to apply mutations to your immutable records using the generated mutable wrappers.
    - [x] Also includes `CreateDraft` and `FinishDraft` methods for more granular control...
    - [x] ...and `AsMutable` and `ToImmutable` extension methods for collections.

## How Mutty Works

Mutty uses a custom attribute `[MutableGeneration]` to mark immutable records for which you want to generate mutable wrappers.
The Incremental Source Generator detects these records and generates corresponding mutable wrapper classes and extension methods.

The basic idea is that with Mutty, you will apply all your changes to a temporary mutable wrapper, which acts as a proxy of the immutable record.
Once all your mutations are completed, Mutty will produce the next immutable state based on the mutations to the mutable wrapper.
This means that you can interact with your data by simply modifying it while keeping all the benefits of immutable data.

![Mutty Overview](docs/images/mutty-overview.png)

Using Mutty is like having a personal assistant. The assistant takes a letter (the current state) and gives you a copy (mutable wrapper) to jot changes onto.
Once you are done, the assistant will take your draft and produce the real immutable, final letter for you (the next state).

### Example Usage

Suppose you have the following immutable records:

```csharp
namespace Mutty.ConsoleApp;

[MutableGeneration]
public record Student(string Email, StudentDetails Details, ImmutableList<Enrollment> Enrollments);

[MutableGeneration]
public record StudentDetails(string Name, int Age);

[MutableGeneration]
public record Enrollment(Course Course, DateTime EnrollmentDate);

[MutableGeneration]
public record Course(string Title, string Description, ImmutableList<Module> Modules);

[MutableGeneration]
public record Module(string Name, ImmutableList<Lesson> Lessons);

[MutableGeneration]
public record Lesson(string Title, string Content);
```

> **Note**: For simplicity, this example focuses on the `Student` record, but Mutty also generates similar mutable wrappers for `StudentDetails`, `Enrollment`, `Course`, `Module`, and `Lesson`.

When you add the `[MutableGeneration]` attribute to your records, Mutty will automatically generate the corresponding mutable wrapper classes:

```csharp
// <auto-generated />
// This file is auto-generated by Mutty.

using System.Collections.Immutable;

namespace Mutty.ConsoleApp
{
    /// <summary>
    /// The mutable wrapper for the <see cref="Student"/> record.
    /// </summary>
    public partial class MutableStudent
    {
        private Student _record;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableStudent"/> class.
        /// </summary>
        /// <param name="record">The record to wrap.</param>
        public MutableStudent(Student record)
        {
            _record = record;

            Email = _record.Email;
            Details = _record.Details;
            Enrollments = _record.Enrollments.AsMutable();
        }

        /// <summary>
        /// Builds a new instance of the <see cref="Student"/> class.
        /// </summary>
        public Student Build()
        {
            return _record with
            {
                Email = this.Email,
                Details = this.Details,
                Enrollments = this.Enrollments.ToImmutable(),
            };
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Student"/> to <see cref="MutableStudent"/>.
        /// </summary>
        public static implicit operator MutableStudent(Student record)
        {
            return new MutableStudent(record);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="MutableStudent"/> to <see cref="Student"/>.
        /// </summary>
        public static implicit operator Student(MutableStudent mutable)
        {
            return mutable.Build();
        }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Record Details.
        /// </summary>
        public MutableStudentDetails Details { get; set; }

        /// <summary>
        /// Gets or sets the ImmutableCollection Enrollments.
        /// </summary>
        public List<MutableEnrollment> Enrollments { get; set; }
    }
}
```

### How to Use the Generated Code

Once the code is generated, you can use the mutable wrappers to modify your immutable records as needed.

#### Deep Nesting Example

Here's an example demonstrating how easy it is to handle deeply nested structures using Mutty:

```csharp
public sealed class ExampleImmutableArray : ExampleBase
{
    public override void Run()
    {
        DisplayHeader("ImmutableArray Example");

        // Initialize original immutable objects
        Student student = Factories.CreateJohnDoe();

        // Use the Produce method to create an updated student object with mutations
        Student updatedStudent = student.Produce(mutable =>
        {
            // Modify the title of the first lesson in the first module of the first course
            mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "=== NEW TITLE ===";
        });

        // Display the original and updated student objects
        DisplayStudentTree(student, 4);
        DisplayStudentTree(updatedStudent, 4);
    }
}
```

#### Comparison with `with` Notation

Without Mutty, updating deeply nested structures using the `with` expression can become cumbersome and error-prone:

```csharp
// Using 'with' notation
var updatedStudent = student with
{
    Enrollments = student.Enrollments.SetItem(0, student.Enrollments[0] with
    {
        Course = student.Enrollments[0].Course with
        {
            Modules = student.Enrollments[0].Course.Modules.SetItem(0, student.Enrollments[0].Course.Modules[0] with
            {
                Lessons = student.Enrollments[0].Course.Modules[0].Lessons.SetItem(0, student.Enrollments[0].Course.Modules[0].Lessons[0] with
                {
                    Title = "=== NEW TITLE ==="
                })
            })
        }
    })
};
```

Using Mutty, the same operation is simpler and more intuitive:

```csharp
// Using Mutty
Student updatedStudent = student.Produce(mutable =>
{
    mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "=== NEW TITLE ===";
});
```

### Ideal for Flux Architecture

Mutty is an excellent fit for state management patterns like Flux. With Mutty, you can maintain immutable state while easily applying updates through the mutable wrappers. This keeps your state management predictable and efficient, especially in complex applications with deeply nested state.

### Installation

To use Mutty in your project:

1. **Add the Mutty package**:
    - You can add it as a NuGet package (if it's available as a package).

2. **Annotate Your Records**:
    - Simply annotate your records with `[MutableGeneration]` to indicate that Mutty should generate a mutable wrapper for them.

3. **Build Your Project**:
    - The Incremental Source Generator will automatically detect the annotated records and generate the corresponding mutable wrappers and extension methods during the build process.

### Best Practices

- **Immutable by Default**: Use immutable records for your core data models to ensure thread safety and prevent unintended side effects.
- **Mutate with Care**: Use the generated mutable wrappers when you need to make changes, but remember to always convert back to the immutable form before exposing the data.
- **Leverage the Implicit Conversion**: Mutty provides implicit conversions between the immutable and mutable versions of your records, making it easy to switch between the two.

### Contributing

If you want to contribute to Mutty or report issues:

- **GitHub Repository**: [Mutty on GitHub](https://github.com/phmatray/mutty)
- **Issues**: Use the GitHub Issues tab to report bugs or request features.

### License

Mutty is open-source software licensed under the [Apache License 2.0](LICENSE).
