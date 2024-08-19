namespace Mutty.ConsoleApp.Examples;

public sealed class ExampleCreateFinishDraft : ExampleBase
{
    public override void Run()
    {
        DisplayHeader("Basic Example: Manual Mutation");
        
        // Initialize original immutable objects
        Student student = Factories.CreateJohnDoe();

        // Create a mutable version of the person object
        MutableStudent mutable = student.CreateDraft();

        // Mutate properties using the mutable wrapper
        mutable.Email = "jack.doe@example.com";
        mutable.Details.Name = "Jack Doe";
        mutable.Details.Age = 36;

        // Build the updated immutable person object
        Student updatedStudent = mutable.FinishDraft();

        // Display the original and updated person objects
        DisplayStudentTree(student);
        DisplayStudentTree(updatedStudent); 
    }
}