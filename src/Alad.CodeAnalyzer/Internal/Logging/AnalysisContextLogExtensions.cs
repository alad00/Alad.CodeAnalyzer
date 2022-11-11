// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Diagnostics;
using System.Linq;

namespace Alad.CodeAnalyzer.Internal.Logging
{
    public struct LogNamedTypeSymbols
    {
        public LogNamedTypeSymbols(
            INamedTypeSymbol loggerType,
            INamedTypeSymbol loggerExtensionsType,
            INamedTypeSymbol loggerMessageType)
        {
            ILogger = loggerType;
            LoggerExtensions = loggerExtensionsType;
            LoggerMessage = loggerMessageType;
        }

        public INamedTypeSymbol ILogger { get; }

        public INamedTypeSymbol LoggerExtensions { get; }

        public INamedTypeSymbol LoggerMessage { get; }

        public bool IsLoggerType(INamedTypeSymbol type)
        {
            return type.Equals(ILogger, SymbolEqualityComparer.Default) ||
                   type.Equals(LoggerExtensions, SymbolEqualityComparer.Default) ||
                   type.Equals(LoggerMessage, SymbolEqualityComparer.Default);
        }

        public bool IsLogMethod(IMethodSymbol method)
        {
            return IsLoggerType(method.ContainingType) && (
                method.Name.Equals("Define", StringComparison.Ordinal) ||
                method.Name.StartsWith("Log", StringComparison.Ordinal));
        }

        public IParameterSymbol GetMessageParameter(IMethodSymbol method)
        {
            Debug.Assert(IsLogMethod(method));

            return method.Parameters
                .FirstOrDefault(p =>
                    p.Type.SpecialType == SpecialType.System_String && (
                        string.Equals(p.Name, "message", StringComparison.Ordinal) ||
                        string.Equals(p.Name, "messageFormat", StringComparison.Ordinal) ||
                        string.Equals(p.Name, "formatString", StringComparison.Ordinal)
                    ));
        }

        public IParameterSymbol GetExceptionParameter(IMethodSymbol method)
        {
            Debug.Assert(IsLogMethod(method));

            return method.Parameters
                .FirstOrDefault(p => string.Equals(p.Name, "exception", StringComparison.Ordinal));
        }

        public IParameterSymbol GetArgsParameter(IMethodSymbol method)
        {
            Debug.Assert(IsLogMethod(method));

            return method.Parameters
                .FirstOrDefault(p => string.Equals(p.Name, "args", StringComparison.Ordinal));
        }
    
        public IArgumentOperation GetMessageArgument(IInvocationOperation invocation)
        {
            var message = GetMessageParameter(invocation.TargetMethod);
            if (message == null)
                return null;

            return invocation.Arguments
                .FirstOrDefault(a => a.Parameter.Equals(message, SymbolEqualityComparer.Default));
        }

        public IArgumentOperation GetExceptionArgument(IInvocationOperation invocation)
        {
            var exception = GetExceptionParameter(invocation.TargetMethod);
            if (exception == null)
                return null;

            return invocation.Arguments
                .FirstOrDefault(a => a.Parameter.Equals(exception, SymbolEqualityComparer.Default));
        }

        public IArgumentOperation GetArgsArgument(IInvocationOperation invocation)
        {
            var args = GetArgsParameter(invocation.TargetMethod);
            if (args == null)
                return null;

            return invocation.Arguments
                .FirstOrDefault(a => a.Parameter.Equals(args, SymbolEqualityComparer.Default));
        }
    }

    public static class AnalysisContextLogExtensions
    {
        public static void RegisterOperationLogInvocationAction(this AnalysisContext context, Action<OperationAnalysisContext, LogNamedTypeSymbols> action)
        {
            context.RegisterCompilationStartAction(context2 =>
            {
                var wellKnownTypeProvider = new WellKnownTypeProvider(context2.Compilation);

                if (!wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingILogger, out var loggerType) ||
                    !wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerExtensions, out var loggerExtensionsType) ||
                    !wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftExtensionsLoggingLoggerMessage, out var loggerMessageType))
                {
                    return;
                }

                context2.RegisterOperationAction(
                    context3 =>
                    {
                        var typeSymbols = new LogNamedTypeSymbols(
                            loggerType,
                            loggerExtensionsType,
                            loggerMessageType);

                        var invocation = (IInvocationOperation)context3.Operation;

                        // questa action è interessata solo alle chiamate ai metodi di logging
                        if (!typeSymbols.IsLogMethod(invocation.TargetMethod))
                            return;

                        action(context3, typeSymbols);
                    },
                    OperationKind.Invocation);
            });
        }
    }
}
