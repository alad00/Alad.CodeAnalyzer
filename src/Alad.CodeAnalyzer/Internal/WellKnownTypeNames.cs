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

        public const string AladDiagnosticsCodeAnalysisExpectsAwaitAttribute = "Alad.Diagnostics.CodeAnalysis.ExpectsAwaitAttribute";

        public const string MicrosoftEntityFrameworkCoreEntityFrameworkQueryableExtensions = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";

        public const string MicrosoftExtensionsLoggingILogger = "Microsoft.Extensions.Logging.ILogger";
        public const string MicrosoftExtensionsLoggingLoggerExtensions = "Microsoft.Extensions.Logging.LoggerExtensions";
        public const string MicrosoftExtensionsLoggingLoggerMessage = "Microsoft.Extensions.Logging.LoggerMessage";

        public const string SystemIAsyncResult = "System.IAsyncResult";
        public const string SystemThreadStaticAttribute = "System.ThreadStaticAttribute";

        public const string SystemThreadingTasksValueTask = "System.Threading.Tasks.ValueTask";
        public const string SystemThreadingTasksValueTask1 = "System.Threading.Tasks.ValueTask`1";
    }
}
