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
}
