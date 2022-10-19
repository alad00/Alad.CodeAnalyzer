using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LetExceptionsPropagateAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Security.LetExceptionsPropagate,
            title: "Catch di tutte le eccezioni incondizionatamente",
            messageFormat: "Intercetta solo le eccezioni note, lascia che le altre vengano intercettate e gestite esternamente",
            category: nameof(AladDiagnosticCodes.Security),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/tree/master/docs/codes/{AladDiagnosticCodes.Security.LetExceptionsPropagate}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.CatchClause);
        }

        static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            var operation = (ICatchClauseOperation)context.SemanticModel.GetOperation(catchClause);

            // se ha un filtro `when (...)` lasciamo passare
            if (operation.Filter != null)
            {
                // TODO: verificare che il filtro non sia sempre `true`, per esempio `when (true)` oppure `when (ex is Exception)`
                return;
            }

            // se non è un `catch (Exception)` od un `catch (AggregateException)` lasciamo passare
            if (
                operation.ExceptionType.ContainingNamespace.Name != nameof(System) ||
                (operation.ExceptionType.Name != nameof(Exception) && operation.ExceptionType.Name != nameof(AggregateException))
            ) return;

            // se contiene uno statement `throw;` (rethrow senza modificare l'eccezione) lasciamo passare
            var doesRethrow = catchClause.Block.DescendantNodes(n => true).Any(n => n is ThrowStatementSyntax);
            if (doesRethrow)
                return;

            var location = catchClause.CatchKeyword.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
