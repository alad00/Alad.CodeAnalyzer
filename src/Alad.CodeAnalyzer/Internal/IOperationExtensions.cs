// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;

namespace Alad.CodeAnalyzer.Internal
{
    public static class IOperationExtensions
    {
        public static T FindParent<T>(this IOperation operation) where T : IOperation
        {
            IOperation candidate = operation;
            do
            {
                if (candidate is T match)
                    return match;
            }
            while ((candidate = candidate.Parent) != null);

            return default;
        }
    }
}
