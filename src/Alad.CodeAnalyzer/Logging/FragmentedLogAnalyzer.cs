// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.Logging
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FragmentedLogAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Logging.FragmentedLog,
            title: "Il log di un singolo evento non dovrebbe essere suddiviso su più righe",
            messageFormat: "Log di un singolo evento suddiviso su più righe",
            category: nameof(AladDiagnosticCodes.Logging),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.Logging.FragmentedLog}.md");

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
            var expression = invocation.Parent as IExpressionStatementOperation;

            // se non è in un'espressione, è un log anomalo
            if (expression == null)
                return;

            // legge (se presente) l'operazione seguente nello stesso blocco
            var nextOperation = (expression.Parent as IBlockOperation)?.Operations
                .SkipWhile(expr => expr != invocation.Parent).Skip(1)
                .FirstOrDefault();

            // verifica che l'operazione seguente sia un'altra invocazione dello stsso metodo di logging
            // (per verificare che sia lo stesso metodo si controlla solo il nome, non l'intera firma)
            if (!(
                nextOperation is IExpressionStatementOperation e &&
                e.Operation is IInvocationOperation nextInvocation &&
                nextInvocation.TargetMethod.Name == invocation.TargetMethod.Name))
            {
                return;
            }

            // se c'è una riga vuota tra le due invocazioni, sono trattate come log separati (non correlati)
            if (nextInvocation.Syntax.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia)))
                return;

            var location = nextInvocation.Syntax.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
