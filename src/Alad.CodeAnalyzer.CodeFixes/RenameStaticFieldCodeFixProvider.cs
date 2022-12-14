﻿// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameStaticFieldCodeFixProvider)), Shared]
    public class RenameStaticFieldCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName
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
                    title: "Converti nome field statico in formato 's_camelCase' o 't_camelCase'",
                    createChangedSolution: c => Rename(context.Document, declaration, c),
                    equivalenceKey: AladEquivalenceKeys.RenameUnderscoreCamelCase),
                diagnostic);
        }

        async Task<Solution> Rename(Document document, SyntaxNode declaration, CancellationToken cancellationToken)
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var symbol = semanticModel.GetDeclaredSymbol(declaration);

            var isThreadStatic = false;

            var wellKnownTypeProvider = new WellKnownTypeProvider(compilation);
            if (wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadStaticAttribute, out var threadStaticAttributeType))
            {
                isThreadStatic = symbol.GetAttributes().Any(a => a.AttributeClass.Equals(threadStaticAttributeType, SymbolEqualityComparer.Default));
            }

            var solution = await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                symbol,
                default,
                isThreadStatic ? CaseConversions.ToThreadStaticFieldCase(symbol.Name) : CaseConversions.ToStaticFieldCase(symbol.Name));

            return solution;
        }
    }
}
