// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllExceptionsCaughtAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Security.AllExceptionsCaught,
            title: "Intercettare solo le eccezioni note, lasciare che le altre vengano intercettate e gestite esternamente",
            messageFormat: "Catch incondizionato di {0}",
            category: nameof(AladDiagnosticCodes.Security),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/tree/master/docs/codes/{AladDiagnosticCodes.Security.AllExceptionsCaught}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(Analyze, OperationKind.CatchClause);
        }

        static void Analyze(OperationAnalysisContext context)
        {
            var operation = (ICatchClauseOperation)context.Operation;
            var syntax = (CatchClauseSyntax)operation.Syntax;

            // se ha un filtro `when (...)` lasciamo passare
            if (operation.Filter != null)
            {
                // TODO: verificare che il filtro non sia sempre `true`, per esempio `when (true)` oppure `when (ex is Exception)`
                return;
            }

            // se non è un `catch (Exception)` od un `catch (AggregateException)` lasciamo passare
            if (
                operation.ExceptionType.ContainingNamespace.Name != nameof(System) ||
                operation.ExceptionType.Name != nameof(Exception) && operation.ExceptionType.Name != nameof(AggregateException)
            ) return;

            // se contiene uno statement `throw;` (rethrow senza modificare l'eccezione) lasciamo passare
            var doesRethrow = syntax.Block.DescendantNodes(n => true).Any(n => n is ThrowStatementSyntax);
            if (doesRethrow)
                return;

            var location = syntax.CatchKeyword.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, operation.ExceptionType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
