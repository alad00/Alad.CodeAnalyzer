// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.PublicFieldNameAnalyzer,
    Alad.CodeAnalyzer.RenameToPascalCaseCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenamePublicFieldToPascalCaseCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsOnConstructors()
    {
        var test = @"
class _myClass {
    public _myClass() {
    }
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsOnIndexer()
    {
        var test = @"
class MyClass {
    public int this[int i] => 123;
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsOnOperators()
    {
        var test = @"
class MyClass {
    public static implicit operator int(MyClass obj) => 123;
}
";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRenamePublicFieldToPascalCase()
    {
        var test = @"
class MyClass {
    public int {|#0:_myField|};
}";

        var fixtest = @"
class MyClass {
    public int MyField;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PublicFieldName).WithLocation(0).WithArguments("_myField");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameProtectedFieldToPascalCase()
    {
        var test = @"
class MyClass {
    protected int {|#0:_myField|};
}";

        var fixtest = @"
class MyClass {
    protected int MyField;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PublicFieldName).WithLocation(0).WithArguments("_myField");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenamePublicPropertyToPascalCase()
    {
        var test = @"
class MyClass {
    public int {|#0:_myProperty|} { get; set; }
}";

        var fixtest = @"
class MyClass {
    public int MyProperty { get; set; }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PublicFieldName).WithLocation(0).WithArguments("_myProperty");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenamePublicEventToPascalCase()
    {
        var test = @"
using System;

class MyClass {
    public event EventHandler {|#0:_myEvent|};
}";

        var fixtest = @"
using System;

class MyClass {
    public event EventHandler MyEvent;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PublicFieldName).WithLocation(0).WithArguments("_myEvent");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenamePublicMethodToPascalCase()
    {
        var test = @"
class MyClass {
    public void {|#0:_myMethod|}() {
    }
}";

        var fixtest = @"
class MyClass {
    public void {|#0:MyMethod|}() {
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PublicFieldName).WithLocation(0).WithArguments("_myMethod");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
