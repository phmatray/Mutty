# Installation

Installing **Mutty** into your .NET project is straightforward since it’s distributed as a NuGet package. Mutty targets .NET Standard 2.0 (as an analyzer), which is compatible with .NET 5, .NET 6, .NET 7, .NET 8, and later ([
NuGet Gallery
| Mutty
](https://www.nuget.org/packages/Mutty)). Mutty is a development-time analyzer/source-generator package, so nothing ships at runtime. It requires **Roslyn 4.8 or later** — for example Visual Studio 2022 17.8+ or the .NET 8 SDK and newer — which covers the C# record support Mutty depends on.

Follow the steps below to add Mutty to your project:

## .NET CLI Installation

You can install Mutty via the .NET CLI using the `dotnet add package` command:

```bash
dotnet add package Mutty
``` 

This will download and reference the Mutty package in your project. After adding the package, rebuild your project so that the source generator can run and produce the necessary code.

If you prefer, you can also use the Package Manager Console in Visual Studio:

```powershell
Install-Package Mutty
```

Either approach will add the appropriate `<PackageReference>` to your project file.

## Visual Studio (Package Manager UI)

1. **Open NuGet Package Manager:** In Visual Studio, right-click your project in Solution Explorer and choose **Manage NuGet Packages...**.
2. **Browse for Mutty:** In the **Browse** tab, search for “**Mutty**”. You should see the Mutty package in the results.
3. **Install:** Select the Mutty package by Philippe Matray and click **Install**. Confirm any prompts to add the package. Visual Studio will add the package reference to your project.

4. **Build the Project:** Once Mutty is installed, build your project. The source generator will run during compilation to generate the mutable classes for your annotated records.

## Package Reference (Optional Configuration)

Mutty is a source-only tool (an analyzer), so it doesn’t need to be included in your runtime deployment. It’s recommended to mark it with `PrivateAssets="all"` so it won’t flow to downstream consumers (in case you create a library) ([
NuGet Gallery
| Mutty
](https://www.nuget.org/packages/Mutty)). Visual Studio does this by default. If you are manually editing the project file, your package reference should look like:

```xml
<PackageReference Include="Mutty" Version="1.0.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

This ensures Mutty’s files are used only at build time (as an analyzer) and are not packed with your application.

## After Installation

After installing, you can start marking your record types with `[MutableGeneration]` and let Mutty do its work. The next build will produce new source files (visible in Visual Studio under **Dependencies > Analyzers > Mutty** or in the build output) corresponding to your records. You typically won’t need to manually interact with these generated files – you can directly use the provided extension methods and wrappers in your code. Proceed to the [Usage](Usage.md) section for examples of how to use Mutty in practice.
