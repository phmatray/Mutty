// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using Mutty.Benchmarks;

// Run with: dotnet run -c Release --project src/tests/Mutty.Benchmarks
// Quick smoke run:    dotnet run -c Release --project src/tests/Mutty.Benchmarks -- --job Dry
BenchmarkSwitcher.FromAssembly(typeof(GeneratorBenchmarks).Assembly).Run(args);
