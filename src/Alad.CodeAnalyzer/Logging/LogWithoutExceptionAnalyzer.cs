// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Alad.CodeAnalyzer.Internal.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.Logging
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LogWithoutExceptionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Logging.LogWithoutException,
            title: "Le eccezioni dovrebbero essere loggate",
            messageFormat: "Passare l'eccezione come primo parametro del metodo '{0}'",
            category: nameof(AladDiagnosticCodes.Logging),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.Logging.LogWithoutException}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationLogInvocationAction(Analyze);
        }

        static void Analyze(OperationAnalysisContext context, LogNamedTypeSymbols logSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;

            var catchClause = invocation.FindParent<ICatchClauseOperation>();

            // se non è in un blocco `catch` non è rilevante per questo analyzer
            if (catchClause == null)
                return;

            var exception = logSymbols.GetExceptionParameter(invocation.TargetMethod);

            // se il parametro dell'eccezione è già presente, nessun problema (nemmeno se è presente senza un valore)
            if (exception != null)
                return;

            var location = invocation.Syntax.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, invocation.TargetMethod.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
