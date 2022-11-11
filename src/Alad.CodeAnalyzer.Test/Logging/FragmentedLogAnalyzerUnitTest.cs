// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Logging.FragmentedLogAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Visibility;

[TestClass]
public class FragmentedLogAnalyzerUnitTest
{
    const string Scaffold = @"
#nullable enable
namespace Microsoft.Extensions.Logging {
    using System;

    public enum LogLevel { }

    public readonly struct EventId { }

    public interface ILogger {
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

    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"
using Microsoft.Extensions.Logging;

class MyClass {
    MyClass(ILogger logger) {
        logger.LogInformation(""Test"");
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test + Scaffold);
    }

    [TestMethod]
    public async Task DoNotAllowFragmentedLogs()
    {
        var test = @"
using Microsoft.Extensions.Logging;
using System;

class MyClass {
    MyClass(ILogger logger) {
        logger.LogInformation(""Inizio operazione A"");
        {|#0:logger.LogInformation(""Data: {Date}"", DateTime.UtcNow)|};
    }
}
";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Logging.FragmentedLog).WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(test + Scaffold, expected);
    }

    [TestMethod]
    public async Task AllowSubsequentUnreleatedLogs()
    {
        var test = @"
using Microsoft.Extensions.Logging;

class MyClass {
    MyClass(ILogger logger) {
        logger.LogInformation(""Fine operazione A"");

        logger.LogInformation(""Inizio operazione B"");
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test + Scaffold);
    }
}
