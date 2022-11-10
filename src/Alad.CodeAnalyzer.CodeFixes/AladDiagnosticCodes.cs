// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using System;

namespace Alad.CodeAnalyzer
{
    public static class AladEquivalenceKeys
    {
        /// <summary>Lascia che l'eccezione venga propagata.</summary>
        public const string RethrowException = "ALAD_RETHROW";

        /// <summary>Converti field in property.</summary>
        public const string FieldToProperty = "ALAD_FIELD_TO_PROPERTY";
    }
}
