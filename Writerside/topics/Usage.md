# Usage

Once Mutty is installed and referenced in your project, using it involves two basic steps: **(1)** annotate your record types with `[MutableGeneration]` to enable generation of mutable counterparts, and **(2)** use the provided API (such as the `Produce` method or draft methods) to perform mutations on those records. This section walks through examples of these steps, demonstrates deep-nested mutations, compares Mutty’s approach to the built-in `with` expression, and shows how Mutty can be applied in a Flux-style state management scenario.

## Marking Records for Mutation

To get started, mark any immutable record you want to mutate with the `[MutableGeneration]` attribute (which is defined in the Mutty package). This signals the source generator to create a mutable wrapper for that record. For example, consider the following data model:

```C#
using System.Collections.Immutable;
using Mutty; // namespace containing [MutableGeneration]

namespace MyApp.Models
{
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
}
``` 

In this example, each record in the object graph – `Student` and its related records (`StudentDetails`, `Enrollment`, `Course`, `Module`, `Lesson`) – is decorated with `[MutableGeneration]`. During compilation, Mutty will generate a corresponding **mutable class** for each one (e.g. `MutableStudent`, `MutableStudentDetails`, `MutableEnrollment`, etc.). Each mutable class has the same properties as the record, but all fields are now mutable (writable). If a property is another record marked with `[MutableGeneration]`, the mutable class will contain the *mutable* version of that property (for instance, `MutableStudent.Details` will be of type `MutableStudentDetails`). If a property is an immutable collection (like `ImmutableList<Enrollment>`), the mutable class’s property will be a regular `List<MutableEnrollment>` for convenient editing.

> **Note:** The example above shows multiple related records to illustrate nested mutation. Mutty will generate similar wrappers for each annotated record in the chain. For simplicity, let’s focus on using the `Student` record’s wrapper in the following examples – the same principles apply to the other records.

After building the project, you don’t need to manually find or include the generated classes; they are automatically available for use. For instance, you can create a `MutableStudent` by assigning a `Student` to it (the record-to-draft conversion is implicit), via `student.CreateDraft()`, or — most commonly — by using `Produce`, as described below. Converting a draft back to a record is explicit: call `Build()` / `ToImmutable()`.

## Deeply Nested Mutation Example

One of Mutty’s biggest benefits is simplifying updates to deeply nested data structures. Normally, updating nested immutable data requires a lot of repetitive code. With Mutty, you can mutate deeply nested fields through the mutable draft objects directly.

Let’s say we have a `Student` record instance (perhaps loaded from a database or constructed in code):

```C#
Student student = GetStudentFromDatabase();  // an existing immutable student object
```

Now we want to update a deeply nested property – for example, change the title of the first lesson in the first module of the first course that the student is enrolled in. Using Mutty, we can do this in one fluent call with the **`Produce`** extension method:

```C#
Student updatedStudent = student.Produce(mutable =>
{
    // Navigate through the mutable draft and update the nested property:
    mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "Updated Lesson Title";
});
```

In the above code, `student.Produce(...)` creates a mutable draft (`MutableStudent`) from the original `student` record. Inside the lambda, we modify the draft’s nested properties as needed. When the lambda exits, Mutty automatically “finishes” the draft and returns a new `Student` record (`updatedStudent`) with all the changes applied. The original `student` object remains unchanged (as expected with immutable data).

**What happened under the hood?** The `Produce` method used the generated wrappers to let us traverse and modify the data structure in-place. In this case, it accessed `mutable.Enrollments` (which is a `List<MutableEnrollment>`), then the first item’s `Course` (as a `MutableCourse`), then its `Modules` list, and so on, until it reached the `Title` of the `Lesson`. Each of those was a mutable proxy for the original data. After the mutation, Mutty constructed a new `Student` record reflecting the changes. This approach drastically simplifies code for nested updates.

### Comparison with `with` Expression

To appreciate the convenience, compare the Mutty approach with how you would update a deeply nested immutable structure using standard C# `with` expressions and `ImmutableList` operations. Without Mutty, updating the same lesson title would require something like:

```C#
// Without Mutty: using 'with' and ImmutableList methods
var updatedStudent = student with
{
    Enrollments = student.Enrollments.SetItem(0, 
        student.Enrollments[0] with
        {
            Course = student.Enrollments[0].Course with
            {
                Modules = student.Enrollments[0].Course.Modules.SetItem(0, 
                    student.Enrollments[0].Course.Modules[0] with
                    {
                        Lessons = student.Enrollments[0].Course.Modules[0].Lessons.SetItem(0, 
                            student.Enrollments[0].Course.Modules[0].Lessons[0] with
                            {
                                Title = "Updated Lesson Title"
                            }
                        )
                    }
                )
            }
        }
    )
};
```

This code is much more verbose and difficult to read or maintain. Each level of nesting requires a combination of `with` and `SetItem` (for `ImmutableList`) to create a modified copy. It’s easy to make a mistake or get lost in the noise of brackets.

Now contrast that with the Mutty version, which was:

```C#
// With Mutty: using Produce and mutable draft
Student updatedStudent = student.Produce(mutable =>
{
    mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = "Updated Lesson Title";
});
```

This single `Produce` call replaces the entire block of code above, clearly indicating the intent (which field is being changed) without the boilerplate. The Mutty approach improves both clarity and reduces the chance of error when dealing with complex data updates.

## Using Mutty in a Flux Architecture

Mutty is particularly useful in applications that follow a Flux or Redux architecture for state management. In such architectures, you maintain an **immutable application state** and produce new states in response to events (actions) without mutating the existing state. Mutty can simplify the “reducer” logic that produces new states.

For example, imagine an application stores a `Student` record in its state. With Flux, an action might indicate that a lesson title needs to be changed. Using Mutty, the reducer handling that action can do something like:

```C#
// In a Flux/Redux-style reducer:
state.Student = state.Student.Produce(mutable =>
{
    mutable.Enrollments[0].Course.Modules[0].Lessons[0].Title = action.NewTitle;
});
```

Here, `state.Student` is an immutable record in the store. The `.Produce(...)` call generates a new `Student` record with the updated lesson title, and the state is updated to refer to this new record. Throughout this process, the state remains immutable from the perspective of the rest of the application (we never mutate the old `Student` in place, we replace it with a new instance).

By using Mutty, you get the best of both worlds in a Flux architecture: the **predictability and safety of immutability** (previous states remain intact for debugging or time-travel, and no part of the app can inadvertently see a partially mutated state) and the **convenience of direct mutation** when implementing the state transitions. This results in cleaner reducer code and a more intuitive way to express state changes.

Keep in mind that mutable draft objects (like `MutableStudent`) should not be stored or passed around outside the scope of the mutation function. They are meant to be short-lived, used within the `Produce` lambda (or a similar controlled scope) to apply changes. After the new immutable state is produced, work with that new immutable object going forward.
