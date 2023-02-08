// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.NamingConventions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicFieldNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.PublicFieldName,
            title: "Il nome dei membri e dei metodi pubblici deve essere in formato 'PascalCase'",
            messageFormat: "Rinominare '{0}' per rispettare le convenzioni",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.PublicFieldName}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(Analyze, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method, SymbolKind.Event);
        }

        static void Analyze(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;

            // se è generato dal compilatore lasciamo passare
            if (symbol.IsImplicitlyDeclared)
                return;

            // se è un metodo, lo lasciamo passare solo se è ordinario o local function (esclusi quindi getter e setter, costruttori, operatori, ecc...)
            if (symbol is IMethodSymbol m && m.MethodKind != MethodKind.Ordinary && m.MethodKind != MethodKind.LocalFunction)
                return;

            // se è un indexer, non lo lasciamo passare
            if (symbol is IPropertySymbol p && p.IsIndexer)
                return;

            // se non è `public` o `protected` lasciamo passare
            if (!symbol.DeclaredAccessibility.HasFlag(Accessibility.Public) && !symbol.DeclaredAccessibility.HasFlag(Accessibility.Protected))
                return;

            // se rispetta le convenzioni lasciamo passare
            if (CaseConversions.IsPascalCase(symbol.Name))
                return;

            var location = symbol.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
