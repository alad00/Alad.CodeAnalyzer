using System;

namespace Alad.CodeAnalyzer
{
    /// <summary>
    /// Codici diagnostici Alad.
    /// </summary>
    public static class AladDiagnosticCodes
    {
        /// <summary>
        /// Sicurezza.
        /// </summary>
        public static class Security
        {
            /// <summary>Catch di tutte le eccezioni incondizionatamente.</summary>
            public const string AllExceptionsCaught = "ALAD0001";

            /// <summary>Eccezione non specializzata.</summary>
            public const string GenericException = "ALAD0002";
        }

        /// <summary>
        /// Visibilità.
        /// </summary>
        public static class Visibility
        {
            /// <summary>API destinate ad uso interno.</summary>
            public const string InternalApiUsage = "ALAD0100";
        }

        /// <summary>
        /// Best practice.
        /// </summary>
        public static class BestPractices
        {
            /// <summary>Metodo statico, ma è prevista dependency-injection.</summary>
            public const string NoDependencyInjection = "ALAD0200";

            /// <summary>Metodo sincrono in un contesto asincrono.</summary>
            public const string SynchronousCall = "ALAD0210";

            /// <summary>Field pubblico esposto.</summary>
            public const string PublicField = "ALAD0220";
        }

        /// <summary>
        /// Convenzioni di denominazione.
        /// </summary>
        public static class NamingConventions
        {
            /// <summary>Nome classe non rispetta le convenzioni.</summary>
            public const string ClassName = "ALAD0300";

            /// <summary>Nome interfaccia non rispetta le convenzioni.</summary>
            public const string InterfaceName = "ALAD0301";

            /// <summary>Nome field pubblico non rispetta le convenzioni.</summary>
            public const string PublicFieldName = "ALAD0302";

            /// <summary>Nome field privato non rispetta le convenzioni.</summary>
            public const string PrivateFieldName = "ALAD0303";

            /// <summary>Nome field privato statico non rispetta le convenzioni.</summary>
            public const string PrivateStaticFieldName = "ALAD0304";

            /// <summary>Nome parametro non rispetta le convenzioni.</summary>
            public const string ArgumentName = "ALAD0305";

            /// <summary>Nome namespace non rispetta le convenzioni.</summary>
            public const string NamespaceName = "ALAD0306";
        }
    }
}
