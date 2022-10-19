using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LetExceptionsPropagateCodeFixProvider)), Shared]
    public class LetExceptionsPropagateCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.Security.LetExceptionsPropagate
        );

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = (CatchClauseSyntax)root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Propaga eccezione",
                    createChangedDocument: c => AddThrow(context.Document, declaration, c),
                    equivalenceKey: AladDiagnosticCodes.Security.LetExceptionsPropagate + "_ADDTHROW"),
                diagnostic);
        }

        async Task<Document> AddThrow(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // se il blocco del `catch` non è vuoto, aggiungiamo un `throw` alla fine
            if (catchClause.Block.ChildNodes().Any())
            {
                var throwStatement = SyntaxFactory.ThrowStatement();
                var newBlock = catchClause.Block.AddStatements(throwStatement);

                var newRoot = root.ReplaceNode(catchClause.Block, newBlock);
                return document.WithSyntaxRoot(newRoot);
            }
            
            // altrimenti se è vuoto possiamo rimuoverlo in toto
            else
            {
                var tryBlock = (TryStatementSyntax)catchClause.Parent;

                // il `try` deve essere rimosso se aveva solo questo blocco `catch` e non aveva un `finally`
                if (tryBlock.Catches.Count == 1 && tryBlock.Finally == null)
                {
                    var newRoot = root.ReplaceNode(tryBlock, tryBlock.Block.Statements.Select(s => s.WithAdditionalAnnotations(Formatter.Annotation)));

                    return document.WithSyntaxRoot(newRoot);
                }

                // altrimenti togliamo solo il blocco del catch
                else
                {
                    var newTryBlock = tryBlock.RemoveNode(catchClause, SyntaxRemoveOptions.AddElasticMarker);

                    var newRoot = root.ReplaceNode(tryBlock, newTryBlock);
                    return document.WithSyntaxRoot(newRoot);
                }
            }
        }
    }
}
