// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Mutty.Tests.Setup;
using NUnit.Framework;
using Shouldly;

namespace Mutty.Tests;

public class MutableStudentTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ShouldCreateMutableStudent()
    {
        // Arrange
        Student student = Factories.CreateJohnDoe();

        // Act
        MutableStudent mutable = new(student);

        // Assert
        mutable.ShouldNotBeNull();
        mutable.Email.ShouldBe("john.doe@example.com");
        mutable.Details.Name.ShouldBe("John Doe");
    }
}
