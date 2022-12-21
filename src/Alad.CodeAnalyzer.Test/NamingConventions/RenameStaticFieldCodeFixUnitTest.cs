// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.PrivateStaticFieldNameAnalyzer,
    Alad.CodeAnalyzer.RenameStaticFieldCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenameStaticFieldCodeFixUnitTest
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
    public static int Field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsProtected()
    {
        var test = @"
class MyClass {
    protected static int Field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsOnConstants()
    {
        var test = @"
class MyClass {
    const int MyConstant = 123;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRenameFieldToStaticFieldCase()
    {
        var test = @"
class MyClass {
    static int {|#0:Field|};

    MyClass() {
        Field = 123;
    }
}";

        var fixtest = @"
class MyClass {
    static int s_field;

    MyClass() {
        s_field = 123;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName).WithLocation(0).WithArguments("Field");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameFieldToThreadStaticFieldCase()
    {
        var test = @"
using System;

class MyClass {
    [ThreadStatic]
    static int {|#0:Field|};

    MyClass() {
        Field = 123;
    }
}";

        var fixtest = @"
using System;

class MyClass {
    [ThreadStatic]
    static int t_field;

    MyClass() {
        t_field = 123;
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName).WithLocation(0).WithArguments("Field");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameInternalFieldToStaticFieldCase()
    {
        var test = @"
class MyClass {
    static internal int {|#0:HelloWorld|};
}";

        var fixtest = @"
class MyClass {
    static internal int s_helloWorld;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName).WithLocation(0).WithArguments("HelloWorld");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenamePrefixToStaticFieldCase()
    {
        var test = @"
class MyClass {
    static int {|#0:ABField123|};
}";

        var fixtest = @"
class MyClass {
    static int s_abField123;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName).WithLocation(0).WithArguments("ABField123");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameSnakeCaseToStaticFieldCase()
    {
        var test = @"
class MyClass {
    static int {|#0:SOME_FIELD_3|};
}";

        var fixtest = @"
class MyClass {
    static int s_someField3;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName).WithLocation(0).WithArguments("SOME_FIELD_3");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
