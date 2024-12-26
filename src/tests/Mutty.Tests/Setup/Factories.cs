// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Mutty.Tests.Setup;

/// <summary>
/// Factory class that creates a student with enrollments.
/// </summary>
public static class Factories
{
    /// <summary>
    /// Creates a student named John Doe with enrollments in an introductory IT course.
    /// </summary>
    /// <returns>The student object.</returns>
    public static Student CreateJohnDoe()
    {
        // Define the courses
        Course introToITCourse = CreateCourse();

        // Define the enrollments
        ImmutableList<Enrollment> enrollments = ImmutableList.Create(
            new Enrollment(introToITCourse, DateTime.Now));

        // Create and return the student
        const string email = "john.doe@example.com";
        StudentDetails details = new("John Doe", 35);
        return new Student(email, details, enrollments);
    }

    private static Course CreateCourse()
    {
        // Define the modules
        CourseModule programming = CreateModuleProgramming();
        CourseModule dataStructures = CreateModuleDataStructures();
        CourseModule algorithms = CreateModuleAlgorithms();

        return new Course(
            "Introduction to IT",
            "A comprehensive introduction to Information Technology.",
            [programming, dataStructures, algorithms]);
    }

    private static CourseModule CreateModuleProgramming()
    {
        return new(
            "Programming Basics",
            [
                    new Lesson("Introduction to Programming", "Learn the basics of programming."),
                    new Lesson("Control Structures", "Learn about if-statements, loops, etc."),
                    new Lesson("Functions", "Learn how to write and use functions.")
                ]);
    }

    private static CourseModule CreateModuleDataStructures()
    {
        return new(
            "Data Structures",
            [
                    new Lesson("Arrays and Lists", "Learn about arrays and lists."),
                    new Lesson("Stacks and Queues", "Understand the concepts of stacks and queues."),
                    new Lesson("Trees and Graphs", "Introduction to trees and graphs.")
                ]);
    }

    private static CourseModule CreateModuleAlgorithms()
    {
        return new(
            "Algorithms",
            [
                    new Lesson("Sorting Algorithms", "Learn about various sorting algorithms."),
                    new Lesson("Searching Algorithms", "Learn about searching techniques."),
                    new Lesson("Algorithm Complexity", "Understand time and space complexity.")
                ]);
    }
}
