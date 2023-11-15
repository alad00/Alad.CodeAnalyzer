// SPDX-FileCopyrightText: 2023 ALAD SRL
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Alad.CodeAnalyzer.Internal
{
    public static class UsingDirectiveSyntaxExtensions
    {
        public static bool IsRoughlyEquivalentTo(this UsingDirectiveSyntax first, UsingDirectiveSyntax second)
        {
            if ((first.StaticKeyword != default) != (second.StaticKeyword != default))
                return false;

            return first.Name.IsEquivalentTo(second.Name);
        }
    }
}
