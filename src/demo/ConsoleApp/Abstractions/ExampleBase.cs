// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace Mutty.ConsoleApp.Abstractions;

/// <summary>
/// Base class for all examples.
/// </summary>
public abstract class ExampleBase
{
    /// <summary>
    /// Abstract method that each derived class must implement.
    /// </summary>
    public abstract void Run();

    /// <summary>
    /// Common method to display a header for the example.
    /// </summary>
    /// <param name="header">The header to display.</param>
    protected static void DisplayHeader(string header)
    {
        AnsiConsole.MarkupLine($"[bold yellow]{header}[/]\n");
    }

    /// <summary>
    /// Common method to display the properties of a student.
    /// </summary>
    /// <param name="student">The student to display.</param>
    /// <param name="maxDepth">The maximum depth to display.</param>
    protected static void DisplayStudentTree(Student student, int maxDepth = 1)
    {
        // Create the tree root
        var root = new Tree($"[bold yellow]Student: {student.Details.Name}[/]");

        // Add student details
        var detailsNode = root.AddNode("[blue]Details[/]");
        detailsNode.AddNode($"[green]Email:[/] {student.Email}");
        detailsNode.AddNode($"[green]Age:[/] {student.Details.Age}");

        // Add enrollments
        var enrollmentsNode = root.AddNode("[blue]Enrollments[/]");

        foreach (var enrollment in student.Enrollments)
        {
            AddEnrollmentNode(enrollment, enrollmentsNode, maxDepth, 1);
        }

        // Render the tree
        AnsiConsole.Write(root);
    }

    private static void AddEnrollmentNode(Enrollment enrollment, TreeNode parentNode, int maxDepth, int currentDepth)
    {
        var enrollmentNode = parentNode.AddNode($"[yellow]Course: {enrollment.Course.Title}[/]");
        enrollmentNode.AddNode($"[green]Enrollment Date:[/] {enrollment.EnrollmentDate.ToShortDateString()}");
        enrollmentNode.AddNode($"[green]Description:[/] {enrollment.Course.Description}");

        if (currentDepth < maxDepth)
        {
            var modulesNode = enrollmentNode.AddNode("[blue]Modules[/]");
            foreach (var module in enrollment.Course.Modules)
            {
                AddModuleNode(module, modulesNode, maxDepth, currentDepth + 1);
            }
        }
        else
        {
            enrollmentNode.AddNode($"[orange1]Modules: {enrollment.Course.Modules.Count} items[/]");
        }
    }

    private static void AddModuleNode(CourseModule courseModule, TreeNode parentNode, int maxDepth, int currentDepth)
    {
        var moduleNode = parentNode.AddNode($"[yellow]Module: {courseModule.Name}[/]");

        if (currentDepth < maxDepth)
        {
            var lessonsNode = moduleNode.AddNode("[blue]Lessons[/]");
            foreach (var lesson in courseModule.Lessons)
            {
                AddLessonNode(lesson, lessonsNode, maxDepth, currentDepth + 1);
            }
        }
        else
        {
            moduleNode.AddNode($"[orange1]Lessons: {courseModule.Lessons.Count} items[/]");
        }
    }

    private static void AddLessonNode(Lesson lesson, TreeNode parentNode, int maxDepth, int currentDepth)
    {
        var lessonNode = parentNode.AddNode($"[yellow]Lesson: {lesson.Title}[/]");

        if (currentDepth < maxDepth)
        {
            lessonNode.AddNode($"[green]Content:[/] {lesson.Content}");
        }
    }
}
