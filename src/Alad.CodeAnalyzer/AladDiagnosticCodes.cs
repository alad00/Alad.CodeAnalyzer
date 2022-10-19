using System;

namespace Alad.CodeAnalyzer
{
    public static class AladDiagnosticCodes
    {
        public static class Security
        {
            /// <summary>Non fare <see langword="catch"/> di tutte le <see cref="Exception"/> indiscriminatamente.</summary>
            public const string LetExceptionsPropagate = "ALAD0001";
        }
    }
}
