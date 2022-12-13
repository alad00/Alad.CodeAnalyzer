// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.PrivateFieldNameAnalyzer,
    Alad.CodeAnalyzer.RenameToUnderscoreCamelCaseCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenameToUnserscoreCamelCaseCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsPublic()
    {
        var test = @"
class MyClass {
    public int Field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsProtected()
    {
        var test = @"
class MyClass {
    protected int Field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsConstant()
    {
        var test = @"
class MyClass {
    const int Constant = 123;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsInEnum()
    {
        var test = @"
enum MyEnum {
    Field0,
    Field1 = 1,
    Field2,
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRenameFieldToUnderscoreCamelCase()
    {
        var test = @"
class MyClass {
    int {|#0:Field|};

    MyClass() {
        Field = 123;
    }
}";

        var fixtest = @"
class MyClass {
    int _field;

    MyClass() {
        _field = 123;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateFieldName).WithLocation(0).WithArguments("Field");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameInternalFieldToUnderscoreCamelCase()
    {
        var test = @"
class MyClass {
    internal int {|#0:HelloWorld|};
}";

        var fixtest = @"
class MyClass {
    internal int _helloWorld;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateFieldName).WithLocation(0).WithArguments("HelloWorld");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenamePrefixToUnderscoreCamelCase()
    {
        var test = @"
class MyClass {
    int {|#0:ABField123|};
}";

        var fixtest = @"
class MyClass {
    int _abField123;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateFieldName).WithLocation(0).WithArguments("ABField123");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameSnakeCaseToUnderscoreCamelCase()
    {
        var test = @"
class MyClass {
    int {|#0:SOME_FIELD_3|};
}";

        var fixtest = @"
class MyClass {
    int _someField3;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateFieldName).WithLocation(0).WithArguments("SOME_FIELD_3");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
