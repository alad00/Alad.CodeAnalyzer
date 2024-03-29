﻿// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Test.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Logging.FragmentedLogAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Visibility;

[TestClass]
public class FragmentedLogAnalyzerUnitTest
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
        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code, expected);
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

        await VerifyCS.VerifyAnalyzerAsync(test + LoggingStubs.Code);
    }
}
