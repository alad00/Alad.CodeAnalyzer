// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.Security.AllExceptionsCaughtAnalyzer,
    Alad.CodeAnalyzer.RethrowExceptionCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.Security;

[TestClass]
public class RethrowExceptionCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRemoveCatch()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } catch (ArgumentException) {
            Console.WriteLine(""Argument error"");
        } {|#0:catch|} (Exception e) {
            // empty
        }
    }
}";

        var fixtest = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } catch (ArgumentException) {
            Console.WriteLine(""Argument error"");
        }
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.AllExceptionsCaught).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRemoveTryCatch()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        try {
            // test
            Console.WriteLine(""Hello, World!"");
        } {|#0:catch|} (Exception e) {
            // empty
        }
    }
}";

        var fixtest = @"
using System;

class MyClass {
    void Test() {
        // test
        Console.WriteLine(""Hello, World!"");
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.AllExceptionsCaught).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixAddThrow()
    {
        var test = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } {|#0:catch|} (Exception e) {
            Console.WriteLine(""Error"");
        }
    }
}";

        var fixtest = @"
using System;

class MyClass {
    void Test() {
        try {
            Console.WriteLine(""Hello, World!"");
        } catch (Exception e) {
            Console.WriteLine(""Error"");
            throw;
        }
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.AllExceptionsCaught).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixReplaceReturnWithThrow()
    {
        var test = @"
using System;

class MyClass {
    bool Test() {
        try {
            Console.WriteLine(""Hello, World!"");
            return true;
        } {|#0:catch|} (Exception e) {
            Console.WriteLine(""Error"");
            return false;
        }
    }
}";

        var fixtest = @"
using System;

class MyClass {
    bool Test() {
        try {
            Console.WriteLine(""Hello, World!"");
            return true;
        } catch (Exception e) {
            Console.WriteLine(""Error"");
            throw;
        }
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.AllExceptionsCaught).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRemoveReturn()
    {
        var test = @"
using System;

class MyClass {
    bool Test() {
        try {
            Console.WriteLine(""Hello, World!"");
            return true;
        } {|#0:catch|} (Exception) {
            return false;
        }
    }
}";

        var fixtest = @"
using System;

class MyClass {
    bool Test() {
        Console.WriteLine(""Hello, World!"");
        return true;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.AllExceptionsCaught).WithLocation(0).WithArguments("Exception");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
