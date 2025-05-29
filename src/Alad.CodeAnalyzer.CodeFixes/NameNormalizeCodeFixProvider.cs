// SPDX-FileCopyrightText: 2025 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NameNormalizeCodeFixProvider)), Shared]
    public class NameNormalizeCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AladDiagnosticCodes.NamingConventions.NameSpecialCharacters
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

            var name = diagnostic.Properties["name"];
            if (CanBeNormalized(name))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Normalizza nome",
                        createChangedSolution: c => Rename(context.Document, declaration, c),
                        equivalenceKey: AladEquivalenceKeys.NameNormalize),
                    diagnostic);
            }
        }

        async Task<Solution> Rename(Document document, SyntaxNode declaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var symbol = semanticModel.GetDeclaredSymbol(declaration) ?? semanticModel.GetSymbolInfo(declaration).Symbol;

            if (!CanBeNormalized(symbol.Name))
                return document.Project.Solution;

            var solution = await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                symbol,
                default,
                Normalize(symbol.Name));

            return solution;
        }

        static string Normalize(string name)
            => new string(name.Normalize(NormalizationForm.FormKD).Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());

        static bool CanBeNormalized(string name)
            => !IsNormalized(name) && IsNormalized(Normalize(name));

        static bool IsNormalized(string name)
            => name.All(c => c is '_' || (c <= (char)127 && char.IsLetterOrDigit(c)));
    }
}
