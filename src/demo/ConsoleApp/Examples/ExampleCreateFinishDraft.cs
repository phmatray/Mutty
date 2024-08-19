// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Examples;

/// <summary>
/// Basic example that demonstrates CreateDraft and FinishDraft methods.
/// </summary>
public sealed class ExampleCreateFinishDraft : ExampleBase
{
    /// <inheritdoc />
    public override void Run()
    {
        DisplayHeader("Basic Example: Manual Mutation");

        // Initialize original immutable objects
        var student = Factories.CreateJohnDoe();

        // Create a mutable version of the person object
        var mutable = student.CreateDraft();

        // Mutate properties using the mutable wrapper
        mutable.Email = "jack.doe@example.com";
        mutable.Details.Name = "Jack Doe";
        mutable.Details.Age = 36;

        // Build the updated immutable person object
        var updatedStudent = mutable.FinishDraft();

        // Display the original and updated person objects
        DisplayStudentTree(student);
        DisplayStudentTree(updatedStudent);
    }
}
