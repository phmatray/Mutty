namespace Mutty.ConsoleApp.Abstractions;

public static class Factories
{
    public static Student CreateJohnDoe()
    {
        // Define the courses
        var introToITCourse = CreateCourse();

        // Define the enrollments
        var enrollments = ImmutableList.Create(
            new Enrollment(introToITCourse, DateTime.Now)
        );

        // Create and return the student
        const string email = "john.doe@example.com";
        var details = new StudentDetails("John Doe", 35);
        return new Student(email, details, enrollments);
    }
    
    public static Course CreateCourse()
    {
        // Define the modules
        var programming = CreateModuleProgramming();
        var dataStructures = CreateModuleDataStructures();
        var algorithms = CreateModuleAlgorithms();

        return new Course(
            "Introduction to IT",
            "A comprehensive introduction to Information Technology.",
            [programming]
        );
    }
    
    public static Module CreateModuleProgramming()
    {
        return new Module("Programming Basics", [
            new Lesson("Introduction to Programming", "Learn the basics of programming."),
            new Lesson("Control Structures", "Learn about if-statements, loops, etc."),
            new Lesson("Functions", "Learn how to write and use functions.")
        ]);
    }
    
    public static Module CreateModuleDataStructures()
    {
        return new Module("Data Structures", [
            new Lesson("Arrays and Lists", "Learn about arrays and lists."),
            new Lesson("Stacks and Queues", "Understand the concepts of stacks and queues."),
            new Lesson("Trees and Graphs", "Introduction to trees and graphs.")
        ]);
    }
    
    public static Module CreateModuleAlgorithms()
    {
        return new Module("Algorithms", [
            new Lesson("Sorting Algorithms", "Learn about various sorting algorithms."),
            new Lesson("Searching Algorithms", "Learn about searching techniques."),
            new Lesson("Algorithm Complexity", "Understand time and space complexity.")
        ]);
    }
}