// SPDX-FileCopyrightText: Microsoft
// SPDX-FileCopyrightText: .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2022 ALAD SRL
//
// SPDX-License-Identifier: MIT

namespace Alad.CodeAnalyzer.Internal
{
    public static class WellKnownTypeNames
    {
        // https://github.com/dotnet/roslyn-analyzers/blob/a9c5683ad67c8a1b06b3f804c409032641498cca/src/Utilities/Compiler/WellKnownTypeNames.cs

        public const string MicrosoftExtensionsLoggingILogger = "Microsoft.Extensions.Logging.ILogger";
        public const string MicrosoftExtensionsLoggingLoggerExtensions = "Microsoft.Extensions.Logging.LoggerExtensions";
        public const string MicrosoftExtensionsLoggingLoggerMessage = "Microsoft.Extensions.Logging.LoggerMessage";

        public const string SystemThreadStaticAttribute = "System.ThreadStaticAttribute";
    }
}
