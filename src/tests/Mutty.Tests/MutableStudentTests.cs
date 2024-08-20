// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using Mutty.Tests.Setup;
using NUnit.Framework;

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
        var student = Factories.CreateJohnDoe();

        // Act
        var mutable = new MutableStudent(student);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(mutable, Is.Not.Null);
            Assert.That(mutable.Email, Is.EqualTo("john.doe@example.com"));
            Assert.That(mutable.Details.Name, Is.EqualTo("John Doe"));
        });
    }
}
