// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace Alad.CodeAnalyzer.Internal
{
    public static class SyntaxNodeExtensions
    {
        public static T FindParent<T>(this SyntaxNode node) where T : SyntaxNode
        {
            SyntaxNode candidate = node;
            do
            {
                if (candidate is T match)
                    return match;
            }
            while ((candidate = candidate.Parent) != null);

            return null;
        }
    }
}
