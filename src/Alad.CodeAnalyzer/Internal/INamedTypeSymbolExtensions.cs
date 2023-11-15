// SPDX-FileCopyrightText: 2023 ALAD SRL
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace Alad.CodeAnalyzer.Internal
{
    public static class INamedTypeSymbolExtensions
    {
        public static bool ExtendsType(this INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            var candidate = type.BaseType;
            while (candidate != null)
            {
                if (candidate.Equals(baseType, SymbolEqualityComparer.Default))
                    return true;

                candidate = candidate.BaseType;
            }

            return false;
        }
    }
}
