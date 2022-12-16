// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

namespace Alad.CodeAnalyzer.Test.NamingConventions.Internal;

public static class RecordStubs
{
    public const string Code = @"
#nullable enable
namespace System.Runtime.CompilerServices {
    public static class IsExternalInit { }
}
#nullable restore
";
}
