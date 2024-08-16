using Mutty.ConsoleApp.Abstractions;

namespace Mutty.ConsoleApp.Examples;

public sealed class ExampleBasic : ExampleBase
{
    public override void Run()
    {
        DisplayHeader("Basic Example: Manual Mutation");
        
        // Initialize original immutable objects
        Student student = CreateITStudent("John Doe", 35);

        // Create a mutable version of the person object
        var mutable = new MutableStudent(student);

        // Mutate properties using the mutable wrapper
        mutable.Email = "jack.doe@example.com";
        mutable.Details.Name = "Jack Doe";
        mutable.Details.Age = 36;

        // Build the updated immutable person object
        var updatedStudent = mutable.Build();

        // Display the original and updated person objects
        DisplayStudentTree(student);
        DisplayStudentTree(updatedStudent); 
    }
}