// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Test.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Logging.DecorativeLogAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Visibility;

[TestClass]
public class DecorativeLogAnalyzerUnitTest
{
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

        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code);
    }

    [TestMethod]
    public async Task NoDiagnosticsExpectedOnBrackets()
    {
        var test = @"
using Microsoft.Extensions.Logging;

class MyClass {
    MyClass(ILogger logger) {
        logger.LogInformation(""{Count} values added."", 3);
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code);
    }

    [TestMethod]
    public async Task DoNotAllowDecorativeLogs()
    {
        var test = @"
using Microsoft.Extensions.Logging;
using System;

class MyClass {
    MyClass(ILogger logger) {
        logger.LogInformation({|#0:""------ MyClass ------""|});
    }
}
";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Logging.DecorativeLog).WithLocation(0);
        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code, expected);
    }
}
