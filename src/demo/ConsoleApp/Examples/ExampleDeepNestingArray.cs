// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Examples;

/// <summary>
/// Example that demonstrates deep nesting of immutable arrays.
/// </summary>
public sealed class ExampleDeepNestingArray : ExampleBase
{
    /// <inheritdoc />
    public override void Run()
    {
        DisplayHeader("ImmutableArray Example");

        // Initialize original immutable objects
        var student = Factories.CreateJohnDoe();

        // Use the Produce method to create an updated person object with mutations
        var updatedStudent = student.Produce(mutable =>
        {
            // You can use the Add, Insert, Remove, and RemoveAt methods to mutate the immutable list
            mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "=== NEW TITLE ===";
            mutable.Enrollments[0].Course.Modules[0].Lessons[0].Content = "=== NEW CONTENT ===";
        });

        // Display the original and updated person objects
        DisplayStudentTree(student, 4);
        DisplayStudentTree(updatedStudent, 4);
    }
}
