# API Reference

This section provides a detailed reference for the code elements that Mutty generates and the helper methods it provides. When you annotate a record with `[MutableGeneration]`, Mutty’s source generator creates several things:

- A **mutable wrapper class** for the record (e.g. `MutableStudent` for a record `Student`).
- An **implicit operator** from the record to its wrapper (create a draft) and an **explicit operator** back (build the record).
- A **`Build()` method** — and a **`ToImmutable()`** alias — that produces the immutable record, plus a **`partial void OnBeforeBuild()`** validation hook.
- Chainable **`With<Property>`** fluent setters.
- Extension methods: **`Produce`**, **`CreateDraft`**, **`FinishDraft`** for creating and using drafts, and **`AsMutable`**, **`ToImmutable`** for working with collections.

Below we describe each of these in detail.

## `MutableGeneration` Attribute

**Namespace:** `Mutty`  
**Usage:** Place `[MutableGeneration]` above an immutable record definition to indicate that Mutty should generate a mutable wrapper for it.

The attribute itself contains no properties – it’s simply a marker. It can be applied to record types (and should *only* be used on records intended to be immutable). There’s no need to add this attribute to classes or structs (Mutty is designed for `record` types specifically). Once a record is annotated, on the next build the Mutty generator will pick it up and produce the associated code.

## Generated Mutable Wrapper Classes

For each annotated record, Mutty generates a partial class named `Mutable<RecordName>` in the **same namespace** as the original record. For example, given `public record Student(...)` with `[MutableGeneration]`, a class `public partial class MutableStudent` is created.

Key characteristics of a mutable wrapper class:

- **Fields and Properties:** It contains public auto-properties for each property of the original record, with identical names but types adjusted for mutability. Value types and immutable types remain the same (e.g. `string Email` stays `string Email`), whereas reference types that are themselves records annotated with `[MutableGeneration]` become their mutable counterparts (e.g. `StudentDetails Details` becomes `MutableStudentDetails Details` in `MutableStudent`). Immutable collection types (like `ImmutableList<T>`) are converted to mutable collections (like `List<MutableT>` if `T` is an annotated record, or `List<T>` if `T` is a value type or non-annotated reference). This allows you to add/remove items or modify contained records directly.

- **Constructor:** The wrapper has a constructor that takes the original record as a parameter. This constructor initializes the mutable object by copying over all values from the record into the wrapper’s properties. For nested records and collections, it will initialize the mutable properties by converting the original nested data into their mutable forms (e.g. by calling the appropriate `AsMutable()` on an immutable list, or implicit conversion on a nested record). After construction, the mutable object is a deep copy (in mutable form) of the original record’s state.

- **Build Method:** Each mutable class has a method `public <RecordType> Build()` which reconstructs the immutable record from the mutable state. `Build()` first calls the `OnBeforeBuild()` hook, then uses the original record and the C# `with` expression to create a new instance with updated properties. Internally it converts collections back to their immutable counterparts and calls each nested wrapper’s `Build()` explicitly. A `ToImmutable()` method is generated as a discoverable alias for `Build()`.

- **`With<Property>` Methods:** For every property, the wrapper generates a chainable setter that assigns the value and returns the wrapper, e.g. `public MutableStudent WithEmail(string value)`. These let you express updates fluently inside a recipe: `student.Produce(d => d.WithEmail("a@b.com").WithAge(31))`.

- **`OnBeforeBuild` Hook:** The wrapper declares `partial void OnBeforeBuild()`, called at the start of `Build()`. Implement it in your own partial class to validate invariants or normalize state in one place.

- **Conversion Operators:** Mutty defines two conversion operators:
    - `public static implicit operator MutableStudent(Student record)` – **implicit**: assigning a `Student` to a `MutableStudent` creates a draft (it just calls the wrapper’s constructor). This is why you can treat a `Student` as a `MutableStudent` without an explicit cast.
    - `public static explicit operator Student(MutableStudent mutable)` – **explicit**: converting a draft back to a record calls `Build()`, which allocates a new record. It is deliberately explicit so the allocation is never hidden (for instance, `record == mutable` does not silently build a record). Prefer calling `Build()` / `ToImmutable()` directly; use `(Student)mutable` only when a cast reads more clearly.

- **Partial Class:** The generated class is declared `partial`, so you can extend it with your own partial class of the same name — most commonly to implement `OnBeforeBuild()` or add helper methods. Don’t try to override the generated members; add new ones.

**Example:** If you have `record Student(string Email, ImmutableList<Enrollment> Enrollments)`, Mutty will generate a class `MutableStudent` with a property `public string Email { get; set; }` and `public List<MutableEnrollment> Enrollments { get; set; }`, among others, plus the conversion operators. This lets you manipulate email or the enrollments list freely on a `MutableStudent` instance. The same pattern applies for all other records.

## `Produce` Extension Method

**Signature (conceptual):**
```C#
public static TRecord Produce<TRecord>(
    this TRecord record,
    Action<MutableTRecord> mutate) 
    where TRecord : class;
``` 

The `Produce` method is an **extension method** generated for each annotated record type. It provides the primary high-level API for using Mutty. The method takes an immutable record instance (`record`) and a lambda `mutate` that receives the mutable version of that record. Within the lambda, you can modify the mutable draft freely. When the lambda exits, `Produce` returns a new immutable record of the same type with all the mutations applied.

- The generic signature above is conceptual – in practice Mutty generates a concrete overload for each of your record types to ensure `mutate` receives the correct `MutableTRecord` type without requiring you to specify type arguments. For example, for a `Student` record, it generates `public static Student Produce(this Student record, Action<MutableStudent> mutate)`.

**Usage:** You typically call `Produce` as `myRecord.Produce(draft => { ...modify draft... });`. Inside the lambda, you work with the `Mutable<Record>` type, which has the same field structure as your record but all fields are settable. You do **not** need to return anything from the lambda; the `Produce` method handles creating the new record for you.

**What it does under the hood:**
1. It takes the original immutable record and implicitly converts it to the mutable wrapper (by calling the implicit operator or constructor).
2. It passes this mutable object into your `mutate` action.
3. Once the action completes, it implicitly converts the mutable object back to the immutable form (using the implicit conversion that calls `Build()`). The new immutable record is then returned.

If no changes are made inside the lambda, the returned record will be effectively equal to the original (since `Build()` will produce the same state). If you do make changes, only those changes will differ – everything else will be copied from the original record.

This method is very convenient for inline modifications, especially for nested data. It also ensures the mutation scope is limited to the provided lambda, making it clear when and where changes occur.

**Example:**
```C#
Order updatedOrder = order.Produce(o => {
    o.Customer.Name = "Alice";
    o.Items[0].Quantity += 1;
});
```  
In this example, `Order` is an immutable record annotated with `[MutableGeneration]`, and it has a nested `Customer` record and a list of `Items`. The `Produce` call creates a `MutableOrder` (`o`) from `order`. We then update the customer’s name and increment the quantity of the first item. When `Produce` returns, `updatedOrder` is a new `Order` record with those changes applied. The original `order` remains unchanged.

## `CreateDraft` and `FinishDraft` Methods

Mutty also provides two complementary methods for scenarios where you want to separate the creation of a draft from the finalization of changes. These are lower-level than `Produce`, but can be useful if you need to perform mutations across multiple methods or conditional logic without staying inside a single lambda.

- **CreateDraft:** This is an extension method that takes an immutable record and returns a new mutable wrapper instance (similar to what `Produce` does initially). For example, `MutableStudent draft = student.CreateDraft();` would produce a `MutableStudent` from a `Student` record. Under the hood, this just calls the implicit conversion (or constructor) to create the mutable object. After calling `CreateDraft`, you are responsible for making the desired changes on the draft and then calling `FinishDraft` to get a new immutable record.

- **FinishDraft:** This extension is the counterpart to `CreateDraft`. It takes a mutable wrapper and produces an immutable record. For example, `Student newStudent = draft.FinishDraft();` will build a new `Student` from a `MutableStudent draft`. Internally, this likely just invokes the implicit conversion back to `Student` (which calls `Build()` on the draft).

Using `CreateDraft`/`FinishDraft` explicitly is equivalent to what `Produce` does, but split into two steps. This gives you more control if needed. For instance:

```C#
// Separate draft usage example:
MutableStudent draft = student.CreateDraft();
// Possibly pass draft to another function or do complex logic
if (someCondition)
    draft.Email = "new.email@example.com";
foreach (var enrollment in draft.Enrollments)
    enrollment.EnrollmentDate = DateTime.Today;
… // other complex mutations
Student modifiedStudent = draft.FinishDraft();
```

In this example, we obtain a `draft` from an original `student`, perform multiple modifications on the draft (even in different scopes or based on conditions), and finally call `FinishDraft` to get the updated `Student`. This approach can improve code clarity when a series of changes can’t be neatly made in a single lambda. It also allows inspecting or intervening in the middle of a mutation process.

**Important:** After calling `FinishDraft`, you should discard the mutable draft object (or at least not use it further). If you call `FinishDraft` again on the same draft, it will just produce another new record with the same state (which might be harmless), but generally the draft is meant to be one-time use. Also, do not forget to call `FinishDraft` – if you create a draft and never finish it, the changes remain only in the mutable object and are not applied to any immutable record.

In summary, use `CreateDraft`/`FinishDraft` when you need fine-grained control, and use `Produce` for concise inline updates. They are different interfaces to the same mechanism.

## `AsMutable` and `ToImmutable` Extension Methods

Mutty generates **collection helper methods** to handle converting between immutable collections and their mutable counterparts. These are especially useful for managing `ImmutableList<T>` or `ImmutableArray<T>` properties within your records.

- **AsMutable():** This extension method is available on immutable collection types (like `ImmutableList<T>`). It produces a mutable copy of the collection. If `T` is a reference type that has a mutable wrapper, `AsMutable()` will convert each element to its mutable form; otherwise, it will just copy the items as-is into a `List<T>`.

  For example, if you have `ImmutableList<Enrollment> enrollments` in your record, the generated wrapper’s constructor will call `enrollments.AsMutable()`, resulting in a `List<MutableEnrollment>`. You can also call this directly in your code if needed. Similarly, it would handle an `ImmutableList<int>` by returning a `List<int>` with the same values.

  In general, `AsMutable` can be thought of as: “give me a modifiable List copy of this immutable list”. This is used internally by Mutty to initialize mutable drafts, but you can use it yourself for other scenarios where you want to convert an immutable collection to a list for editing.

- **ToImmutable():** This extension complements `AsMutable()`. It is available on standard mutable collections like `List<T>` (or more generally, anything that implements `IEnumerable<T>` could be converted, but typically a `List<T>`). It returns an immutable collection (usually by calling `.ToImmutableList()` under the hood) containing the elements. If `T` is a mutable wrapper, it will convert each element back to the immutable type before producing the immutable collection.

  For example, in the `MutableStudent.Build()` method, after you finish editing `List<MutableEnrollment> Enrollments`, it calls `Enrollments.ToImmutable()` which produces an `ImmutableList<Enrollment>` for the new `Student`. Each `MutableEnrollment` in the list is converted to `Enrollment` via implicit conversion, and the result is an `ImmutableList` containing those records.

These methods make it easy to round-trip collections when building or tearing down immutable structures. They are generally used internally by the generated code, but are exposed if you need them in your own utility code.

**Note:** The `AsMutable`/`ToImmutable` extension methods bridge collections of *annotated records* (`ImmutableList<T>` ↔ `List<MutableT>`). The wrapper itself, however, handles a wide range of collection property types directly — `ImmutableArray`, `ImmutableList`, `ImmutableDictionary`/`SortedDictionary`, `ImmutableHashSet`/`SortedSet`, `ImmutableQueue`/`Stack`, plain arrays (`T[]`, defensively copied so they are never aliased), and `IReadOnlyList<T>`/`IReadOnlyCollection<T>` (exposed as `List<T>`). Each is converted to a mutable form on construction and back to the original immutable/array/read-only form in `Build()`.

## Putting It Together – Example

To see the relationship between these APIs, consider again an annotated `Student` record. With Mutty in place, you can do the following in your code:

```C#
Student student = ... // original student
// Use CreateDraft/FinishDraft:
MutableStudent draft = student.CreateDraft();
draft.Age += 1;
draft.Enrollments.Add(new Enrollment(...)); // adding via List<MutableEnrollment>
Student olderStudent = draft.FinishDraft();

// Or use Produce for the same outcome:
Student olderStudent2 = student.Produce(m => {
    m.Age += 1;
    m.Enrollments.Add(new Enrollment(...));
});
```

Both approaches above yield a new `Student` with an incremented age and an additional enrollment, but `Produce` does it in one call. Under the hood, `Produce` was essentially doing the `CreateDraft` and `FinishDraft` steps for you.

Remember that the **generated classes and methods are all static or compile-time** – using Mutty does not add any runtime library dependency aside from the small overhead of copying values. Everything is resolved during compilation. You write code using these helpers as if they existed natively for your types, and the compiler (with Mutty’s help) takes care of the rest.

For more insight into how Mutty generates these and how it works behind the scenes, refer to the [Architecture](Architecture.md) documentation.
