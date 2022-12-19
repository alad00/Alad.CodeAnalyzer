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
    public class NamespaceNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.NamespaceName,
            title: "Il nome dei namespace deve essere in formato 'PascalCase'",
            messageFormat: "Rinominare il namespace '{0}' per rispettare le convenzioni",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.NamespaceName}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(Analyze, SymbolKind.Namespace);
        }

        static void Analyze(SymbolAnalysisContext context)
        {
            var ns = context.Symbol;

            // per il namespace "topmost" va bene tutto in quanto rappresenta un brand,
            // e le direttive Microsoft non prevedono restrizioni sul casing dei brand
            if (ns.ContainingNamespace.IsGlobalNamespace)
                return;

            // se rispetta le convenzioni lasciamo passare
            if (CaseConversions.IsPascalCase(ns.Name))
                return;

            var location = ns.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, ns.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
