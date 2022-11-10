// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RethrowExceptionCodeFixProvider)), Shared]
    public class RethrowExceptionCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.Security.AllExceptionsCaught
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
                    title: "Lascia che l'eccezione venga propagata",
                    createChangedDocument: c => AddThrow(context.Document, declaration, c),
                    equivalenceKey: AladEquivalenceKeys.RethrowException),
                diagnostic);
        }

        async Task<Document> AddThrow(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var catchBlockChildren = catchClause.Block.ChildNodes();
            var catchBlockChildrenExceptReturn = catchBlockChildren.Where(n => !(n is ReturnStatementSyntax));

            // se il blocco del `catch` non è vuoto, aggiungiamo un `throw;` alla fine,
            // se contiene solo un'espressione `return …;` è considerato vuoto
            if (catchBlockChildrenExceptReturn.Any())
            {
                var throwStatement = SyntaxFactory.ThrowStatement();

                BlockSyntax newBlock = catchClause.Block;

                // se termina con un `return …;`, il return viene rimpiazzato dal `throw;`
                var lastNode = catchBlockChildren.LastOrDefault();
                if (lastNode is ReturnStatementSyntax)
                    newBlock = newBlock.RemoveNode(lastNode, SyntaxRemoveOptions.KeepLeadingTrivia);

                // aggiungo `throw;`
                newBlock = newBlock.AddStatements(throwStatement);

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

                // altrimenti togliamo solo il blocco del `catch`
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
