// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.NamingConventions.NamespaceNameAnalyzer,
    Alad.CodeAnalyzer.RenameToPascalCaseCodeFixProvider>;

namespace Alad.CodeAnalyzer.Test.NamingConventions;

[TestClass]
public class RenameNamespaceToPascalCaseCodeFixUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task CodeFixRenameNamespaceToPascalCase()
    {
        var test = @"
namespace brandNAME.{|#0:my_namespace|} {
}";

        var fixtest = @"
namespace brandNAME.MyNamespace {
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NamespaceName).WithLocation(0).WithArguments("my_namespace");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameSubNamespaceToPascalCase()
    {
        var test = @"
namespace brandNAME.MyNamespace.{|#0:something|} {
}";

        var fixtest = @"
namespace brandNAME.MyNamespace.Something {
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NamespaceName).WithLocation(0).WithArguments("something");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    [TestMethod]
    public async Task CodeFixRenameNestedNamespaceToPascalCase()
    {
        var test = @"
namespace brandNAME.MyNamespace {
    namespace {|#0:helloWorld|} {
    }
}";

        var fixtest = @"
namespace brandNAME.MyNamespace {
    namespace HelloWorld {
    }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.NamingConventions.NamespaceName).WithLocation(0).WithArguments("helloWorld");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
