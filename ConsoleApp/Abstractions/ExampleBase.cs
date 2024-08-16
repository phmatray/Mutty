using System.Collections.Immutable;
using Spectre.Console;

namespace Mutty.ConsoleApp.Abstractions;

public abstract class ExampleBase
{
    // Abstract method that each derived class must implement
    public abstract void Run();

    // Factory method to create a student with IT courses
    protected Student CreateITStudent(string studentName, int age)
    {
        // Define the lessons
        var programmingBasicsLessons = ImmutableList.Create(
            new Lesson("Introduction to Programming", "Learn the basics of programming."),
            new Lesson("Control Structures", "Learn about if-statements, loops, etc."),
            new Lesson("Functions", "Learn how to write and use functions.")
        );

        var dataStructuresLessons = ImmutableList.Create(
            new Lesson("Arrays and Lists", "Learn about arrays and lists."),
            new Lesson("Stacks and Queues", "Understand the concepts of stacks and queues."),
            new Lesson("Trees and Graphs", "Introduction to trees and graphs.")
        );

        var algorithmsLessons = ImmutableList.Create(
            new Lesson("Sorting Algorithms", "Learn about various sorting algorithms."),
            new Lesson("Searching Algorithms", "Learn about searching techniques."),
            new Lesson("Algorithm Complexity", "Understand time and space complexity.")
        );

        // Define the modules
        var programmingModule = new Module("Programming Basics", programmingBasicsLessons);
        var dataStructuresModule = new Module("Data Structures", dataStructuresLessons);
        var algorithmsModule = new Module("Algorithms", algorithmsLessons);

        // Define the courses
        var introToITCourse = new Course(
            "Introduction to IT",
            "A comprehensive introduction to Information Technology.",
            ImmutableList.Create(programmingModule, dataStructuresModule, algorithmsModule)
        );

        // Define the enrollments
        var enrollments = ImmutableList.Create(
            new Enrollment(introToITCourse, DateTime.Now)
        );

        // Create and return the student
        var email = $"{studentName.Replace(" ", "-").ToLower()}@example.com";
        var details = new StudentDetails(studentName, age);
        return new Student(email, details, enrollments);
    }

    // Common method to display a header for the example
    protected void DisplayHeader(string header)
    {
        AnsiConsole.MarkupLine($"[bold yellow]{header}[/]\n");
    }

    // Common method to display the properties of a student
    protected void DisplayStudentTree(Student student, int maxDepth = 1)
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

    private void AddEnrollmentNode(Enrollment enrollment, TreeNode parentNode, int maxDepth, int currentDepth)
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

    private void AddModuleNode(Module module, TreeNode parentNode, int maxDepth, int currentDepth)
    {
        var moduleNode = parentNode.AddNode($"[yellow]Module: {module.Name}[/]");

        if (currentDepth < maxDepth)
        {
            var lessonsNode = moduleNode.AddNode("[blue]Lessons[/]");
            foreach (var lesson in module.Lessons)
            {
                AddLessonNode(lesson, lessonsNode, maxDepth, currentDepth + 1);
            }
        }
        else
        {
            moduleNode.AddNode($"[orange1]Lessons: {module.Lessons.Count} items[/]");
        }
    }

    private void AddLessonNode(Lesson lesson, TreeNode parentNode, int maxDepth, int currentDepth)
    {
        var lessonNode = parentNode.AddNode($"[yellow]Lesson: {lesson.Title}[/]");

        if (currentDepth < maxDepth)
        {
            lessonNode.AddNode($"[green]Content:[/] {lesson.Content}");
        }
    }
}