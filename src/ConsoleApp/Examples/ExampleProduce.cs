namespace Mutty.ConsoleApp.Examples;

public sealed class ExampleProduce : ExampleBase
{
    public override void Run()
    {
        DisplayHeader("Produce Example: Fluent Mutation");

        // Initialize original immutable objects
        Student student = Factories.CreateJohnDoe();

        // Use the Produce method to create an updated person object with mutations
        Student updatedStudent = student.Produce(mutable =>
        {
            mutable.Email = "jack.doe@example.com";
            mutable.Details.Name = "Jack Doe";
            mutable.Details.Age = 36;
        });

        // Display the original and updated person objects
        DisplayStudentTree(student);
        DisplayStudentTree(updatedStudent);
    }
}