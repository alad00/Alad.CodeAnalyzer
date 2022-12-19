// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

namespace Alad.CodeAnalyzer
{
    public static class AladEquivalenceKeys
    {
        /// <summary>Lascia che l'eccezione venga propagata.</summary>
        public const string RethrowException = "ALAD_RETHROW";

        /// <summary>Converti field in property.</summary>
        public const string FieldToProperty = "ALAD_FIELD_TO_PROPERTY";

        /// <summary>Converti nome field in formato '_camelCase'.</summary>
        public const string RenameUnderscoreCamelCase = "ALAD_RENAME_UNDERSCORE_CAMEL_CASE";

        /// <summary>Converti nome field in formato 'PascalCase'.</summary>
        public const string RenamePascalCase = "ALAD_RENAME_PASCAL_CASE";

        /// <summary>Converti nome parametro in formato 'camelCase'.</summary>
        public const string RenameCamelCase = "ALAD_RENAME_CAMEL_CASE";
    }
}
