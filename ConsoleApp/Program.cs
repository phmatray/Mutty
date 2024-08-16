using Mutty.ConsoleApp;

var address = new Address("123 Main St", "Wonderland");
var person = new Person("Alice", 30, address);

var mutablePerson = new MutablePerson(person);
mutablePerson.Name = "Bob";
mutablePerson.Age = 35;
mutablePerson.Address.Street = "456 Elm St";
var updatedPerson = mutablePerson.Build();

// Output: Person { Name = Alice, Age = 30, Address = Address { Street = 123 Main St, City = Wonderland } }
Console.WriteLine(person);

// Output: Person { Name = Bob, Age = 35, Address = Address { Street = 456 Elm St, City = Wonderland } }
Console.WriteLine(updatedPerson);
