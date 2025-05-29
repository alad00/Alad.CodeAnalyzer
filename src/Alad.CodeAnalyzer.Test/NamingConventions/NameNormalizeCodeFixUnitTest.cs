// SPDX-FileCopyrightText: 2025 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.AlphanumericNameAnalyzer,
    Alad.CodeAnalyzer.NameNormalizeCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class NameNormalizeCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixNormalizeClassNames()
    {
        var test = @"
class {|#0:Entità|} {

    Entità() {
    }
}";

        var fixtest = @"
class Entita {

    Entita() {
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NameSpecialCharacters).WithLocation(0).WithArguments("Entità");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixReportPropertyNames()
    {
        var test = @"
class Test {
    public int {|#0:果物|} { get; set; }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NameSpecialCharacters).WithLocation(0).WithArguments("果物");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task CodeFixReportParameterNames()
    {
        var test = @"
class Test {
    public int Banana { get; set; }

    Test(int {|#0:バナナ|}) {
        Banana = バナナ;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NameSpecialCharacters).WithLocation(0).WithArguments("バナナ");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task CodeFixNormalizeMethodNames()
    {
        var test = @"
class Test {
    public int Banana { get; set; }

    public void {|#0:AttivaEntità|}() {
        Banana = 42;
    }
}";

        var fixtest = @"
class Test {
    public int Banana { get; set; }

    public void AttivaEntita() {
        Banana = 42;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NameSpecialCharacters).WithLocation(0).WithArguments("AttivaEntità");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixNormalizeVariableNames()
    {
        var test = @"
class Test {
    public void Hello() {
        int {|#0:entità|} = 123;
    }
}";

        var fixtest = @"
class Test {
    public void Hello() {
        int entita = 123;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NameSpecialCharacters).WithLocation(0).WithArguments("entità");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
