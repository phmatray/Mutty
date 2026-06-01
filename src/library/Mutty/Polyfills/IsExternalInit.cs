// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Apache 2.0 license.
// See the LICENSE file in the project root for full license information.

namespace System.Runtime.CompilerServices;

/// <summary>
/// Compiler shim required to use <c>init</c> accessors and records when targeting netstandard2.0,
/// which does not ship this type. Marked internal so it never leaks from the generator assembly.
/// </summary>
internal static class IsExternalInit
{
}
