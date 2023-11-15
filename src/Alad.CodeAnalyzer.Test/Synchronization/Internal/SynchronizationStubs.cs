// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

namespace Alad.CodeAnalyzer.Test.Synchronization.Internal;

public static class SynchronizationStubs
{
    public const string Code = @"
#nullable enable
namespace Alad.Diagnostics.CodeAnalysis {
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ExpectsAwaitAttribute : Attribute {
        public ExpectsAwaitAttribute(bool required = true) {
        }
    }
}
#nullable restore
";
}
