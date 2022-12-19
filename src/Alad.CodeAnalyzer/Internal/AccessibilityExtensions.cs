// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace Alad.CodeAnalyzer.Internal
{
    public static class AccessibilityExtensions
    {
        public static string GetCSharpName(this Accessibility accessibility)
        {
            if (accessibility.HasFlag(Accessibility.Public))
                return "public";

            string s = null;

            if (accessibility.HasFlag(Accessibility.Protected))
                s = "protected";

            if (accessibility.HasFlag(Accessibility.Internal))
                s = (s == null ? "" : $"{s} ") + "internal";

            if (s == null)
                return "private";

            return s;
        }
    }
}
