using System.Collections.Immutable;

namespace Mutty.ConsoleApp;

[MutableGeneration]
public record Student(string Email, StudentDetails Details, ImmutableList<Enrollment> Enrollments);

[MutableGeneration]
public record StudentDetails(string Name, int Age);

[MutableGeneration]
public record Enrollment(Course Course, DateTime EnrollmentDate);

[MutableGeneration]
public record Course(string Title, string Description, ImmutableList<Module> Modules);

[MutableGeneration]
public record Module(string Name, ImmutableList<Lesson> Lessons);

[MutableGeneration]
public record Lesson(string Title, string Content);

