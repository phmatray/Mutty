using System.Collections.Immutable;

namespace Mutty.ConsoleApp;

public record Student(string Email, StudentDetails Details, ImmutableList<Enrollment> Enrollments);

public record StudentDetails(string Name, int Age);

public record Enrollment(Course Course, DateTime EnrollmentDate);

public record Course(string Title, string Description, ImmutableList<Module> Modules);

public record Module(string Name, ImmutableList<Lesson> Lessons);

public record Lesson(string Title, string Content);

