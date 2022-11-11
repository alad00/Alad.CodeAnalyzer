// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

namespace Alad.CodeAnalyzer.Test.Logging.Internal;

public static class LoggingStubs
{
    public const string Code = @"
#nullable enable
namespace Microsoft.Extensions.Logging {
    using System;

    public enum LogLevel { }

    public readonly struct EventId { }

    public interface ILogger {
        IDisposable BeginScope<TState>(TState state);
        bool IsEnabled(LogLevel logLevel);
        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
    }

    public static class LoggerExtensions {
        public static void LogInformation(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args) { }
        public static void LogInformation(this ILogger logger, EventId eventId, string? message, params object?[] args) { }
        public static void LogInformation(this ILogger logger, Exception? exception, string? message, params object?[] args) { }
        public static void LogInformation(this ILogger logger, string? message, params object?[] args) { }
    }

    public static class LoggerMessage {
    }
}
#nullable restore
";
}
