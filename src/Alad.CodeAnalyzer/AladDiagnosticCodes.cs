using System;

namespace Alad.CodeAnalyzer
{
    public static class AladDiagnosticCodes
    {
        public static class Security
        {
            /// <summary>Catch di tutte le eccezioni incondizionatamente.</summary>
            public const string AllExceptionsCaught = "ALAD0001";

            /// <summary>Eccezione non specializzata.</summary>
            public const string GenericException = "ALAD0002";
        }
    }
}
