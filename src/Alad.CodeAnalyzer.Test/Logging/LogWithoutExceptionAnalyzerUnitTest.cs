// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Test.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Logging.LogWithoutExceptionAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Visibility;

[TestClass]
public class LogWithoutExceptionAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"
using Microsoft.Extensions.Logging;
using System;

class MyClass {
    MyClass(ILogger logger) {
        try {
            // something
        } catch (FormatException ex) {
            logger.LogInformation(ex, ""Test"");
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code);
    }

    [TestMethod]
    public async Task ExceptionsMustBeLoggerProperly()
    {
        var test = @"
using Microsoft.Extensions.Logging;
using System;

class MyClass {
    MyClass(ILogger logger) {
        try {
            // something
        } catch (FormatException ex) {
            {|#0:logger.LogInformation(""Test: {Exception}"", ex)|};
        }
    }
}
";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Logging.LogWithoutException).WithLocation(0).WithArguments("LogInformation");
        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code, expected);
    }

    [Ignore] // TODO: fix
    [TestMethod]
    public async Task ExceptionsDoNotNeedToBeLoggedTwice()
    {
        var test = @"
using Microsoft.Extensions.Logging;
using System;

class MyClass {
    MyClass(ILogger logger) {
        try {
            // something
        } catch (FormatException ex) {
            if (ex.Message == ""Campo 'esempio' non valido."") {
                logger.LogInformation(""Campo 'esempio' non valido, skip operazione..."");
                return;
            }

            logger.LogInformation(ex, ""Test"");
        }
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code);
    }
}
