// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Alad.CodeAnalyzer.Visibility
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InternalApiUsageAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Visibility.InternalApiUsage,
            title: "Non fare uso di API destinate a consumo interno",
            messageFormat: "Utilizzo di {0} destinato ad uso interno",
            category: nameof(AladDiagnosticCodes.Security),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/tree/master/docs/codes/{AladDiagnosticCodes.Visibility.InternalApiUsage}.md");

        static readonly string NamespaceSeparator = ".";
        static readonly char[] NamespaceSeparators = new[] { '.' };
        static readonly Regex InternalNamespace = new Regex(@"\.Internal(?:\.|$)", RegexOptions.Compiled);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeName, SyntaxKind.QualifiedName);
            context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
        }

        static void AnalyzeName(SyntaxNodeAnalysisContext context)
        {
            var node = (QualifiedNameSyntax)context.Node;

            // prendiamo in analisi i namespace "Internal"
            if (node.Right.Identifier.Value as string != "Internal")
                return;

            var semanticModel = context.SemanticModel;

            // cerco la classe e l'operazione in corso
            IOperation operation = null;
            INamedTypeSymbol currentClass = null;
            SyntaxNode candidate = node;
            do
            {
                if (operation == null)
                    operation = semanticModel.GetOperation(candidate);

                if (currentClass == null && candidate is ClassDeclarationSyntax cd)
                    currentClass = semanticModel.GetDeclaredSymbol(cd);
            }
            while ((currentClass == null || operation == null) && (candidate = candidate.Parent) != null);

            // se non si tratta di un'operazione lasciamo andare
            if (operation == null)
                return;

            // se fa parte dello stesso progetto lasciamo passare
            // Nota: prendiamo solo i primi due componenti del namespace, supponendo che il primo sia l'autore ed il secondo il progetto, esempio: "MyCompany.MyProject.Models"
            var namespaceString = operation.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            var containingNamespace = currentClass.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            if (containingNamespace.Split(NamespaceSeparators, 3).Take(2).SequenceEqual(namespaceString.Split(NamespaceSeparators, 3).Take(2)))
                return;

            var location = operation.Syntax.GetLocation();
            var fullName = string.Join(NamespaceSeparator, new[] { namespaceString, operation.Type.Name }.Where(x => !string.IsNullOrEmpty(x)));
            var diagnostic = Diagnostic.Create(Rule, location, fullName);
            context.ReportDiagnostic(diagnostic);
        }

        static void AnalyzeUsing(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingDirectiveSyntax)context.Node;

            // prendiamo in analisi i namespace "Internal"
            // TODO: gestire casi in cui non è un `QualifiedNameSyntax` (quali casi? as esempio se è un alias?)
            if ((node.Name as QualifiedNameSyntax)?.Right.Identifier.Value as string != "Internal")
                return;

            // TODO: using
        }
    }
}
