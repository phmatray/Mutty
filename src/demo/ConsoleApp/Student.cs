// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp;

/// <summary>
/// Represents a student with a unique email, details, and a list of enrollments.
/// </summary>
/// <param name="Email">The unique email of the student.</param>
/// <param name="Details">The details of the student.</param>
/// <param name="Enrollments">The list of enrollments for the student.</param>
[MutableGeneration]
public record Student(string Email, StudentDetails Details, ImmutableList<Enrollment> Enrollments);

/// <summary>
/// Represents the details of a student with a name and age.
/// </summary>
/// <param name="Name">The name of the student.</param>
/// <param name="Age">The age of the student.</param>
[MutableGeneration]
public record StudentDetails(string Name, int Age);

/// <summary>
/// Represents an enrollment in a course with a course and enrollment date.
/// </summary>
/// <param name="Course">The course being enrolled in.</param>
/// <param name="EnrollmentDate">The date of the enrollment.</param>
[MutableGeneration]
public record Enrollment(Course Course, DateTime EnrollmentDate);

/// <summary>
/// Represents a course with a title, description, and list of modules.
/// </summary>
/// <param name="Title">The title of the course.</param>
/// <param name="Description">The description of the course.</param>
/// <param name="Modules">The list of modules in the course.</param>
[MutableGeneration]
public record Course(string Title, string Description, ImmutableList<CourseModule> Modules);

/// <summary>
/// Represents a module with a name and list of lessons.
/// </summary>
/// <param name="Name">The name of the module.</param>
/// <param name="Lessons">The list of lessons in the module.</param>
[MutableGeneration]
public record CourseModule(string Name, ImmutableList<Lesson> Lessons);

/// <summary>
/// Represents a lesson with a title and content.
/// </summary>
/// <param name="Title">The title of the lesson.</param>
/// <param name="Content">The content of the lesson.</param>
[MutableGeneration]
public record Lesson(string Title, string Content);
