// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Test.Logging.Internal;
using Alad.CodeAnalyzer.Test.NamingConventions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.ParameterNameAnalyzer,
    Alad.CodeAnalyzer.RenameToCamelCaseCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenameToCamelCaseCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task NoDiagnosticsWhenRecordConstructor()
    {
        var test = @"
record MyRecord(int SomeProperty);";

        await VerifyCS.VerifyAnalyzerAsync(test + RecordStubs.Code);
    }

    [TestMethod]
    public async Task CodeFixRenameParameter()
    {
        var test = @"
class MyClass {
    MyClass() {
        MyMethod(SomeParameter: 123);
    }

    void MyMethod(int {|#0:SomeParameter|} = 0) {
    }
}";

        var fixtest = @"
class MyClass {
    MyClass() {
        MyMethod(someParameter: 123);
    }

    void MyMethod(int someParameter = 0) {
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.ParameterName).WithLocation(0).WithArguments("SomeParameter");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameParameterInRecord()
    {
        var test = @"
record MyRecord() {
    void MyMethod(int {|#0:SomeParameter|}) {
    }
}";

        var fixtest = @"
record MyRecord() {
    void MyMethod(int someParameter) {
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.ParameterName).WithLocation(0).WithArguments("SomeParameter");
        await VerifyCS.VerifyCodeFixAsync(test + RecordStubs.Code, expected, fixtest + RecordStubs.Code);
    }
}
