// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alad.CodeAnalyzer.Internal
{
    public static class UsingHelper
    {
        public static Task<Document> AddUsingAsync(Document document, UsingDirectiveSyntax usingDirective, CancellationToken cancellationToken = default)
            => AddUsingAsync(document, usingDirective, default, cancellationToken);

        public static async Task<Document> AddUsingAsync(Document document, UsingDirectiveSyntax usingDirective, TextSpan source, CancellationToken cancellationToken = default)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            var lastUsingBeforeSource = FindDeepestUsingAt(root, source);
            if (lastUsingBeforeSource != null)
            {
                var usings = GetUsings(lastUsingBeforeSource);

                // TODO: inserire mantenendo l'ordinamento alfabetico (tenendo conto di System che a volte è prima in base alle impostazioni).
                editor.InsertAfter(usings.Last(), usingDirective);
            }
            else
            {
                editor.InsertBefore(root.DescendantNodes().FirstOrDefault(), usingDirective);
            }

            return editor.GetChangedDocument();
        }

        static UsingDirectiveSyntax FindDeepestUsingAt(SyntaxNode root, TextSpan source)
        {
            var candidate = root.FindNode(source);
            while (candidate != null)
            {
                var usings = candidate.DescendantNodes().OfType<UsingDirectiveSyntax>();
                if (usings.Any())
                    return usings.LastOrDefault(u => u.SpanStart < source.Start) ?? usings.Last();

                candidate = candidate.Parent;
            }

            return null;
        }

        static IEnumerable<UsingDirectiveSyntax> GetUsings(UsingDirectiveSyntax lastUsingInBlock)
        {
            var current = lastUsingInBlock;
            while (current != null)
            {
                yield return current;
                
                // Prende tutti gli using da quello indicato in su, finché non trova qualcosa di diverso da uno using.
                current = current.Parent?.ChildNodes().TakeWhile(n => n != current).LastOrDefault() as UsingDirectiveSyntax;
            }
        }
    }
}
