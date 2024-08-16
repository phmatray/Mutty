using Mutty.ConsoleApp;

var address = new Address("123 Main St", "Wonderland");
var company = new Company("Acme", address);
var person = new Person("Alice", 30, company);

var mutablePerson = new MutablePerson(person);
mutablePerson.Name = "Bob";
mutablePerson.Age = 35;
mutablePerson.Company.Address.Street = "456 Elm St";
var updatedPerson = mutablePerson.Build();

// Output: Person {
//   Name = Alice,
//   Age = 30,
//   Company = Company {
//     Name = Acme,
//     Address = Address { Street = 123 Main St, City = Wonderland }
//   }
Console.WriteLine(person);

// Output: Person {
//   Name = Bob,
//   Age = 35,
//   Company = Company {
//     Name = Acme,
//     Address = Address { Street = 456 Elm St, City = Wonderland }
//   }
Console.WriteLine(updatedPerson);
