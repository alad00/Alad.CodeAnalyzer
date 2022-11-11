// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpAnalyzerVerifier<
    Alad.CodeAnalyzer.Visibility.InternalApiUsageAnalyzer>;

namespace Alad.CodeAnalyzer.Test.Visibility;

[TestClass]
public class InternalApiUsageAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task DoNotAllowInternalOperation()
    {
        var test = @"
namespace MyCompany.SomeProject {
    class MyClass {
        void Test() {
            {|#0:new OtherProject.Internal.Something()|};
        }
    }
}

namespace MyCompany.OtherProject.Internal {
    public class Something { }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Visibility.InternalApiUsage).WithLocation(0).WithArguments("MyCompany.OtherProject.Internal.Something");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task AllowInternalOperationSameProject()
    {
        var test = @"
namespace MyCompany.SomeProject {
    class MyClass {
        void Test() {
            new Internal.Something();
        }
    }
}

namespace MyCompany.SomeProject.Internal {
    public class Something { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Ignore]  // TODO: fix
    [TestMethod]
    public async Task DoNotAllowInternalUsing()
    {
        var test = @"
namespace MyCompany.SomeProject {
    using {|#0:MyCompany.OtherProject.Internal|};

    class MyClass {
        void Test() {
            new Something();
        }
    }
}

namespace MyCompany.OtherProject.Internal {
    public class Something { }
}";

        var expected = VerifyCS.Diagnostic(AladDiagnosticCodes.Visibility.InternalApiUsage).WithLocation(0).WithArguments("MyCompany.OtherProject.Internal.Something");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task AllowInternalUsingSameProject()
    {
        var test = @"
namespace MyCompany.SomeProject {
    using MyCompany.SomeProject.Internal;

    class MyClass {
        void Test() {
            new Something();
        }
    }
}

namespace MyCompany.SomeProject.Internal {
    public class Something { }
}";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
