// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Security.GenericExceptionAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Security;

[TestClass]
public class GenericExceptionThrownAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task ThrowExceptionStatement()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        throw new {|#0:Exception|}(""Test"");
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.GenericException).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task ThrowExceptionExpression()
    {
        var test = @"
using System;

class MyClass {
    int Test(int x) {
        return x switch {
            123 => 1,
            _ => throw new {|#0:Exception|}(""Test"")
        };
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.GenericException).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task SupportsImplicitObjectCreations()
    {
        var test = @"
using System;

class MyClass {
    int Test(int x) {
        Exception ex = {|#0:new|}(""Test"");
        throw ex;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.GenericException).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenForwarding()
    {
        var test = @"
using System;

class MyClass {
    int Test(int x) {
        throw Ex();
    }

    Exception Ex() => new ArgumentNullException();
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
