// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameToPascalCaseCodeFixProvider)), Shared]
    public class AddExpectsAwaitAttributeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.Synchronization.ExpectsAwaitTaint
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

            var declaration = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Aggiungi attributo '[ExpectsAwait]'.",
                    createChangedSolution: c => AddAttribute(context.Document, declaration, c),
                    equivalenceKey: AladEquivalenceKeys.AddExpectsAwaitAttribute),
                diagnostic);
        }

        async Task<Solution> AddAttribute(Document document, SyntaxNode declaration, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var methodDeclaration = declaration.FindParent<MethodDeclarationSyntax>();

            var attributes = methodDeclaration.AttributeLists.Add(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ExpectsAwait"))
                )));

            var document2 = document.WithSyntaxRoot(
                root.ReplaceNode(
                    methodDeclaration,
                    methodDeclaration.WithAttributeLists(attributes)
                ));

            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Alad.Diagnostics.CodeAnalysis"));
            var document3 = await UsingHelper.AddUsingAsync(document2, usingDirective, declaration.Span, cancellationToken);

            return document3.Project.Solution;
        }
    }
}
