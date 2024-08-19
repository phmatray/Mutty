// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Examples;

/// <summary>
/// Example that demonstrates the Produce method for fluent mutation.
/// </summary>
public sealed class ExampleProduce : ExampleBase
{
    /// <inheritdoc />
    public override void Run()
    {
        DisplayHeader("Produce Example: Fluent Mutation");

        // Initialize original immutable objects
        var student = Factories.CreateJohnDoe();

        // Use the Produce method to create an updated person object with mutations
        var updatedStudent = student.Produce(static mutable =>
        {
            mutable.Email = "jack.doe@example.com";
            mutable.Details.Name = "Jack Doe";
            mutable.Details.Age = 36;
        });

        // Display the original and updated person objects
        DisplayStudentTree(student);
        DisplayStudentTree(updatedStudent);
    }
}
