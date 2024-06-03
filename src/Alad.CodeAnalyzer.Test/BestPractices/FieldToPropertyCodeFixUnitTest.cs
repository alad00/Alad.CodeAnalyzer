// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.BestPractices.PublicFieldAnalyzer,
    Alad.CodeAnalyzer.FieldToPropertyCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.BestPractices;

[TestClass]
public class FieldToPropertyCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsPrivate()
    {
        var test = @"
class MyClass {
    int _field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsInternal()
    {
        var test = @"
class MyClass {
    internal int _field;
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenFieldIsConstant()
    {
        var test = @"
class MyClass {
    public const int Constant = 123;
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
    public async Task StaticReadonlyPredefinedObjectInstanceFields()
    {
        var test = @"
class MyClass {
    // BAD
    public static readonly MyClass {|#0:Default|} = new MyClass();
}

struct MyStruct {
    // OK
    public static readonly MyStruct Default = new MyStruct();

    // OK
    public static readonly string EmptyString = """";
}";

        var fixtest = @"
class MyClass {
    // BAD
    public static MyClass Default { get; } = new MyClass();
}

struct MyStruct {
    // OK
    public static readonly MyStruct Default = new MyStruct();

    // OK
    public static readonly string EmptyString = """";
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.BestPractices.PublicField).WithLocation(0).WithArguments("Default", "public");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixConvertPublicFieldToProperty()
    {
        var test = @"
class MyClass {
    public int {|#0:Field|};
}";

        var fixtest = @"
class MyClass {
    public int Field { get; set; }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.BestPractices.PublicField).WithLocation(0).WithArguments("Field", "public");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixConvertProtectedFieldToProperty()
    {
        var test = @"
class MyClass {
    protected int {|#0:Field|};
}";

        var fixtest = @"
class MyClass {
    protected int Field { get; set; }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.BestPractices.PublicField).WithLocation(0).WithArguments("Field", "protected");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixConvertFieldWithValueToProperty()
    {
        var test = @"
class MyClass {
    public int {|#0:Field|} = 123;
}";

        var fixtest = @"
class MyClass {
    public int Field { get; set; } = 123;
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.BestPractices.PublicField).WithLocation(0).WithArguments("Field", "public");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixConvertReadonlyFieldToProperty()
    {
        var test = @"
class MyClass {
    public readonly int {|#0:Field|};
}";

        var fixtest = @"
class MyClass {
    public int Field { get; }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.BestPractices.PublicField).WithLocation(0).WithArguments("Field", "public");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
