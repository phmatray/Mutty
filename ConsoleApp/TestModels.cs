namespace Mutty.ConsoleApp;

public record Person(string Name, int Age, Company Company);

public record Company(string Name, Address Address);

public record Address(string Street, string City);
