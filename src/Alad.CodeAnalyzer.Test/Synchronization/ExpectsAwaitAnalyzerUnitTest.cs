// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Synchronization;
using Alad.CodeAnalyzer.Test.Synchronization.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Alad.CodeAnalyzer.Test.CSharpCodeFixVerifier<
    Alad.CodeAnalyzer.Synchronization.ExpectsAwaitAnalyzer,
    Alad.CodeAnalyzer.AddExpectsAwaitAttributeFixProvider>;

namespace Alad.CodeAnalyzer.Test.Synchronization;

[TestClass]
public class ExpectsAwaitAnalyzerUnitTest
{
    [TestMethod]
    public async Task NoDiagnosticsExpected()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        _ = Example1Async();
        await Example2Async();
    }

    async Task Example1Async() {
    }

    [ExpectsAwait]
    async Task Example2Async() {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code);
    }

    [TestMethod]
    public async Task PassthroughDoesNotRequireAwait()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    Task Outer1Async() {
        return InnerAsync();
    }

    [ExpectsAwait]
    Task Outer2Async() => InnerAsync();

    [ExpectsAwait]
    async Task InnerAsync() {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code);
    }

    [TestMethod]
    public async Task TaintedCallWithoutAwait()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        _ = {|#0:ExampleAsync()|};
    }

    [ExpectsAwait]
    async Task ExampleAsync() {
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwait);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("ExampleAsync");
        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code, expected);
    }

    [TestMethod]
    public async Task TaintedCallWithSynchronousAccess()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        var result = ExampleAsync().Result;
    }

    [ExpectsAwait]
    async Task<int> ExampleAsync() {
        return 0;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code);
    }

    [Ignore]  // TODO: fix
    [TestMethod]
    public async Task TaintedCallWithoutWaitWithMemberAccess()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        var status = {|#0:ExampleAsync()|}.Status;
    }

    [ExpectsAwait]
    async Task<int> ExampleAsync() {
        return 0;
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("ExampleAsync");
        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code, expected);
    }

    [TestMethod]
    public async Task UntaintedCallWithoutAwait()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    async Task Test() {
        _ = ExampleAsync();
    }

    [ExpectsAwait(false)]
    async Task ExampleAsync() {
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code);
    }

    [TestMethod]
    public async Task TaintedCallsWithoutForward()
    {
        var test = @"
using System.Threading.Tasks;

class MyClass {
    async Task Test() {
        await {|#0:ExampleAsync()|};
    }

    [Alad.Diagnostics.CodeAnalysis.ExpectsAwait]
    async Task ExampleAsync() {
    }
}";

        var fixtest = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        await ExampleAsync();
    }

    [Alad.Diagnostics.CodeAnalysis.ExpectsAwait]
    async Task ExampleAsync() {
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("ExampleAsync");
        await VerifyCS.VerifyCodeFixAsync(test + SynchronizationStubs.Code, expected, fixtest + SynchronizationStubs.Code);
    }

    [TestMethod]
    public async Task DoesNotAddUsingTwice()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    async Task Test() {
        await {|#0:ExampleAsync()|};
    }

    [ExpectsAwait]
    async Task ExampleAsync() {
    }
}";

        var fixtest = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        await ExampleAsync();
    }

    [ExpectsAwait]
    async Task ExampleAsync() {
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("ExampleAsync");
        await VerifyCS.VerifyCodeFixAsync(test + SynchronizationStubs.Code, expected, fixtest + SynchronizationStubs.Code);
    }

    [TestMethod]
    public async Task EntityFrameworkCoreWithoutAwait()
    {
        var test = @"
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

class MyClass {
    async Task Test(IQueryable<int> test) {
        _ = {|#0:test.CountAsync()|};
    }
}

namespace Microsoft.EntityFrameworkCore {
    public static class EntityFrameworkQueryableExtensions {
        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwait);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("CountAsync");
        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code, expected);
    }

    [TestMethod]
    public async Task TaintedCallToValueTaskWithoutAwait()
    {
        var test = @"
using System.Threading.Tasks;
using Alad.Diagnostics.CodeAnalysis;

class MyClass {
    [ExpectsAwait]
    async Task Test() {
        _ = {|#0:ExampleAsync()|};
    }

    [ExpectsAwait]
    async ValueTask<int> ExampleAsync() {
        return 0;
    }
}";

        var diagnostic = new ExpectsAwaitAnalyzer().SupportedDiagnostics.First(d => d.Id == AladDiagnosticCodes.Synchronization.ExpectsAwait);
        var expected = VerifyCS.Diagnostic(diagnostic).WithLocation(0).WithArguments("ExampleAsync");
        await VerifyCS.VerifyAnalyzerAsync(test + SynchronizationStubs.Code, expected);
    }
}
