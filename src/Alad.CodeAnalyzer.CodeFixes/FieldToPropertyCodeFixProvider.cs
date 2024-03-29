﻿// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldToPropertyCodeFixProvider)), Shared]
    public class FieldToPropertyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.BestPractices.PublicField
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

            var declaration = (VariableDeclaratorSyntax)root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Converti field in property",
                    createChangedDocument: c => ConvertToProperty(context.Document, declaration, c),
                    equivalenceKey: AladEquivalenceKeys.FieldToProperty),
                diagnostic);
        }

        async Task<Document> ConvertToProperty(Document document, VariableDeclaratorSyntax variableDeclarator, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            FieldDeclarationSyntax fieldDeclaration = variableDeclarator.FindParent<FieldDeclarationSyntax>();

            SyntaxTokenList modifiers = fieldDeclaration.Modifiers;

            var accessors = new List<AccessorDeclarationSyntax>()
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(fieldDeclaration.SemicolonToken),
            };

            int readOnlyModifierIndex = modifiers.IndexOf(SyntaxKind.ReadOnlyKeyword);
            if (readOnlyModifierIndex != -1)
            {
                modifiers = modifiers.RemoveAt(readOnlyModifierIndex);
            }
            else
            {
                AccessorDeclarationSyntax setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(fieldDeclaration.SemicolonToken);
                accessors.Add(setter);
            }

            PropertyDeclarationSyntax propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                fieldDeclaration.AttributeLists,
                modifiers,
                fieldDeclaration.Declaration.Type,
                null,
                variableDeclarator.Identifier,
                SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)),
                null,
                variableDeclarator.Initializer,
                variableDeclarator.Initializer == null ? default : fieldDeclaration.SemicolonToken
            );

            SyntaxNode newRoot = root.ReplaceNode(fieldDeclaration, propertyDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
