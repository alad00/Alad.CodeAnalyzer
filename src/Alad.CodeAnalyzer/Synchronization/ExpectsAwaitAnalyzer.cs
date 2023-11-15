// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Alad.CodeAnalyzer.Internal.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.Synchronization
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExpectsAwaitAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_noAwait = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Synchronization.ExpectsAwait,
            title: "Questo metodo non supporta più operazioni in parallelo e dovrebbe essere awaited",
            messageFormat: "Aggiungere await sulla risposta del metodo '{0}'",
            category: nameof(AladDiagnosticCodes.Synchronization),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.Synchronization.ExpectsAwait}.md");

        static readonly DiagnosticDescriptor s_tainted = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint,
            title: "Questo metodo chiama metodi che non supportano operazioni in parallelo",
            messageFormat: "Contrassegnare il metodo '{0}' con attributo [AwaitExpected]",
            category: nameof(AladDiagnosticCodes.Synchronization),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_noAwait,
            s_tainted
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context2 =>
            {
                var wellKnownTypeProvider = new WellKnownTypeProvider(context2.Compilation);

                var asyncTypeSymbols = wellKnownTypeProvider.GetAsyncTypeSymbols();

                context2.RegisterOperationAction(
                    ctx => Analyze(ctx, asyncTypeSymbols),
                    OperationKind.Invocation);
            });
        }

        static void Analyze(OperationAnalysisContext context, AsyncTypeSymbols asyncTypeSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;

            // deve essere una invocazione di metodo asincrono
            if (!asyncTypeSymbols.IsAwaitableMethod(invocation.TargetMethod))
                return;

            if (asyncTypeSymbols.ExpectsAwait(invocation.TargetMethod))
            {
                // segnala se non è immediatamente "awaited" e dovrebbe esserlo
                if (!asyncTypeSymbols.IsImmediatelyAwaited(invocation))
                {
                    var location = invocation.Syntax.GetLocation();
                    var diagnostic = Diagnostic.Create(s_noAwait, location, invocation.TargetMethod.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    var currentMethod = context.ContainingSymbol;

                    // segnala se il metodo non "expects await" nonstante contenga chiamate a metodi che "expects await"
                    if (currentMethod is IMethodSymbol m && !asyncTypeSymbols.ExpectsAwait(m))
                    {
                        var location = invocation.Syntax.GetLocation();
                        var diagnostic = Diagnostic.Create(s_tainted, location, invocation.TargetMethod.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
