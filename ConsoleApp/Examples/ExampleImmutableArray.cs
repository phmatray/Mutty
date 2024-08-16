using Mutty.ConsoleApp.Abstractions;

namespace Mutty.ConsoleApp.Examples;

public sealed class ExampleImmutableArray : ExampleBase
{
    public override void Run()
    {
        DisplayHeader("ImmutableArray Example");
        
        // Initialize original immutable objects
        Student student = Factories.CreateJohnDoe();
        
        // Use the Produce method to create an updated person object with mutations
        Student updatedStudent = student.Produce(mutable =>
        {
            // You can use the Add, Insert, Remove, and RemoveAt methods to mutate the immutable list
            mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "=== NEW TITLE ===";
        });

        // Display the original and updated person objects
        DisplayStudentTree(student, 4);
        DisplayStudentTree(updatedStudent, 4);
    }
}