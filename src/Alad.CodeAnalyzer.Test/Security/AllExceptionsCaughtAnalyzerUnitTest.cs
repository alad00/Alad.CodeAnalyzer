// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Security.AllExceptionsCaughtAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Security;

[TestClass]
public class AllExceptionsCaughtAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenRethrowing()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } catch (Exception e) {
            throw;
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenForwarding()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } catch (Exception e) {
            throw new ApplicationException(""Generic error"", e);
        }
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
