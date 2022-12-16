// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.ConstNameAnalyzer,
    Alad.CodeAnalyzer.RenameToPascalCaseCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenameToPascalCaseCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRenameConstToPascalCase()
    {
        var test = @"
class MyClass {
    const int {|#0:SOME_CONSTANT_VALUE|} = 123;

    MyClass() {
        var test = SOME_CONSTANT_VALUE;
    }
}";

        var fixtest = @"
class MyClass {
    const int SomeConstantValue = 123;

    MyClass() {
        var test = SomeConstantValue;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.ConstName).WithLocation(0).WithArguments("SOME_CONSTANT_VALUE");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameEnumValueToPascalCase()
    {
        var test = @"
enum MyEnum {
    {|#0:SOME_VALUE|} = 1,
}

class MyClass {
    MyClass() {
        var test = MyEnum.SOME_VALUE;
    }
}";

        var fixtest = @"
enum MyEnum {
    SomeValue = 1,
}

class MyClass {
    MyClass() {
        var test = MyEnum.SomeValue;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.ConstName).WithLocation(0).WithArguments("SOME_VALUE");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
