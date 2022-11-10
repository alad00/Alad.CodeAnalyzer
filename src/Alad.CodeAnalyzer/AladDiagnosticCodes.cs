// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using System;

namespace Alad.CodeAnalyzer
{
    /// <summary>
    /// Codici diagnostici Alad.
    /// </summary>
    public static class AladDiagnosticCodes
    {
        #region Sicurezza e performance (da ALAD0000 ad ALAD0999)

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
        /// Logging.
        /// </summary>
        public static class Logging
        {
            /// <summary>Log di un singolo evento suddiviso du più righe.</summary>
            public const string FragmentedLog = "ALAD0300";

            /// <summary>Log in un blocco catch senza eccezione.</summary>
            public const string LogWithoutException = "ALAD0301";

            /// <summary>Log ornamentale.</summary>
            public const string DecorativeLog = "ALAD0302";
        }

        #endregion

        #region Pulizia del codice (da ALAD1000 ad ALAD1999)

        /// <summary>
        /// Convenzioni di denominazione.
        /// </summary>
        public static class NamingConventions
        {
            /// <summary>Nome classe non rispetta le convenzioni.</summary>
            public const string ClassName = "ALAD1000";

            /// <summary>Nome interfaccia non rispetta le convenzioni.</summary>
            public const string InterfaceName = "ALAD1001";

            /// <summary>Nome field pubblico non rispetta le convenzioni.</summary>
            public const string PublicFieldName = "ALAD1002";

            /// <summary>Nome field privato non rispetta le convenzioni.</summary>
            public const string PrivateFieldName = "ALAD1003";

            /// <summary>Nome field privato statico non rispetta le convenzioni.</summary>
            public const string PrivateStaticFieldName = "ALAD1004";

            /// <summary>Nome parametro non rispetta le convenzioni.</summary>
            public const string ArgumentName = "ALAD1005";

            /// <summary>Nome namespace non rispetta le convenzioni.</summary>
            public const string NamespaceName = "ALAD1006";
        }

        /// <summary>
        /// Codice superfluo.
        /// </summary>
        public static class SuperfluousCode
        {
            /// <summary>Blocco finally superfluo.</summary>
            public const string EmptyFinallyBlock = "ALAD1100";
        }

        #endregion
    }
}
