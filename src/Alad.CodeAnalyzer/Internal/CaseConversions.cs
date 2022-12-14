// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;

namespace Alad.CodeAnalyzer.Internal
{
    public static class CaseConversions
    {
        /// <summary>
        /// Accetta stringhe nei formati usati comunemente in C# e le converte in token (un token per parola):
        /// <para>
        /// Eventuali prefissi tipo <c>"s_"</c>, <c>"t_"</c> ed <c>"m_"</c> vengono rimossi.
        /// </para>
        /// <para>
        /// Formati accettati:<br/>
        /// <c>"camelCase"</c>,<br/>
        /// <c>"_underscoreCamelCase"</c>,<br/>
        /// <c>"s_staticFieldCase"</c>,<br/>
        /// <c>"t_threadStaticFieldCase"</c>,<br/>
        /// <c>"PascalCase"</c>,<br/>
        /// <c>"SNAKE_CASE"</c>.
        /// </para>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<string> Tokenize(string str)
        {
            bool anyLower = str.Any(char.IsLower);

            // rileva se inizia con un prefisso "s_", "t_", "m_", ecc...
            bool startsWithPrefix = str.Length > 1 && str[1] == '_' && char.IsLower(str[0]);

            int tokenStartIndex = (startsWithPrefix ? 2 : 0);
            bool previousLower = false;
            bool previousUpper = false;
            for (var i = tokenStartIndex; i < str.Length; i++)
            {
                var chr = str[i];

                // NOTA: tutto ciò che non è minuscolo (quindi anche numeri e simboli eccetto separatore) é considerato maiuscola.
                var isSeparator = chr == '_';
                var isLower = char.IsLower(chr);
                var isUpper = !isLower && !isSeparator;

                // separatore = nuovo token
                // cambio da minuscola a maiuscola = nuovo token
                if (isSeparator || (previousLower && isUpper))
                {
                    var length = i - tokenStartIndex;

                    if (length > 0)
                    {
                        var token = str.Substring(tokenStartIndex, length);
                        yield return anyLower ? token : token.ToLower();
                    }

                    tokenStartIndex = i + (chr == '_' ? 1 : 0);
                }

                // cambio da almeno due maiuscole a minuscola = nuovo token
                else if (previousUpper && isLower && tokenStartIndex < i - 1)
                {
                    // "IOException" = ["IO", "Exception"]
                    // "IMyInterface" = ["I", "My", "Interface"]

                    var token = str.Substring(tokenStartIndex, i - 1 - tokenStartIndex);
                    yield return anyLower ? token : token.ToLower();

                    tokenStartIndex = i - 1;
                }

                previousLower = isLower;
                previousUpper = isUpper;
            }

            if (tokenStartIndex != str.Length)
            {
                var token = str.Substring(tokenStartIndex, str.Length - tokenStartIndex);
                yield return anyLower ? token : token.ToLower();
            }
        }

        // NOTA: le conversioni rispettano le direttive Microsoft che prevedono maiuscole solo per gli acronimi di lunghezza inferiore a 3 lettere.

        public static string ToCamelCase(IEnumerable<string> tokens)
            => string.Join("", tokens.Select((t, i) => (i > 0 && t.Length < 3) ? t : t.ToLower()).Select((t, i) => i == 0 ? t : Capitalize(t)));

        public static string ToCamelCase(string str)
            => ToCamelCase(Tokenize(str));

        public static string ToUnderscoreCamelCase(IEnumerable<string> tokens)
            => "_" + ToCamelCase(tokens);

        public static string ToUnderscoreCamelCase(string str)
            => "_" + ToCamelCase(str);

        public static string ToStaticFieldCase(IEnumerable<string> tokens)
            => "s_" + ToCamelCase(tokens);

        public static string ToStaticFieldCase(string str)
            => "s_" + ToCamelCase(str);

        public static string ToThreadStaticFieldCase(IEnumerable<string> tokens)
            => "t_" + ToCamelCase(tokens);

        public static string ToThreadStaticFieldCase(string str)
            => "t_" + ToCamelCase(str);

        public static string ToPascalCase(IEnumerable<string> tokens)
            => string.Join("", tokens.Select(t => t.Length < 3 ? t : t.ToLower()).Select(Capitalize));

        public static string ToPascalCase(string str)
            => ToPascalCase(Tokenize(str));

        public static string ToSnakeCase(IEnumerable<string> tokens)
            => string.Join("_", tokens.Select(t => t.ToUpperInvariant()));

        public static string ToSnakeCase(string str)
            => ToSnakeCase(Tokenize(str));

        public static bool IsCamelCase(string str)
            => str == ToCamelCase(str);

        public static bool IsUnderscoreCamelCase(string str)
            => str.StartsWith("_") && IsCamelCase(str.Substring(1));

        public static bool IsStaticFieldCase(string str)
            => str.StartsWith("s_") && IsCamelCase(str.Substring(2));

        public static bool IsThreadStaticFieldCase(string str)
            => str.StartsWith("t_") && IsCamelCase(str.Substring(2));

        public static bool IsPascalCase(string str)
            => str == ToPascalCase(str);

        public static bool IsSnakeCase(string str)
            => str == ToSnakeCase(str);

        public static string Capitalize(string str)
            => (str.Length == 0 || char.IsUpper(str[0])) ? str : (char.ToUpperInvariant(str[0]) + str.Substring(1));

        public static string Uncapitalize(string str)
            => (str.Length == 0 || char.IsLower(str[0])) ? str : (char.ToLowerInvariant(str[0]) + str.Substring(1));
    }
}
