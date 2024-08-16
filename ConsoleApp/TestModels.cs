namespace Mutty.ConsoleApp;

public record Address(string Street, string City);
public record Person(string Name, int Age, Address Address);

// public class MutablePerson
// {
//     private Person _person;
//     
//     public MutablePerson(string name, int age)
//     {
//         _person = new Person(name, age);
//     }
//     
//     public MutablePerson(Person person)
//     {
//         _person = person;
//     }
//     
//     public string Name
//     {
//         get => _person.Name;
//         set => _person = _person with { Name = value };
//     }
//     
//     public int Age
//     {
//         get => _person.Age;
//         set => _person = _person with { Age = value };
//     }
//     
//     public Person ToImmutable() => _person;
// }