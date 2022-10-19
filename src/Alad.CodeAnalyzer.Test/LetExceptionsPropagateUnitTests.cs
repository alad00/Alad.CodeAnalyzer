using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.Analyzers.LetExceptionsPropagateAnalyzer,
    Alad.CodeAnalyzer.LetExceptionsPropagateCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test;

[TestClass]
public class AladCodeAnalyzerUnitTest
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

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.LetExceptionsPropagate).WithLocation(0);
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

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.LetExceptionsPropagate).WithLocation(0);
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

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Security.LetExceptionsPropagate).WithLocation(0);
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
