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
    public class PrivateFieldNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.PrivateFieldName,
            title: "Il nome dei field internal o private deve essere in formato '_camelCase'",
            messageFormat: "Rinominare il field '{0}' per rispettare le convenzioni",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.PrivateFieldName}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(Analyze, SymbolKind.Field);
        }

        static void Analyze(SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;

            // se è `const` lasciamo passare
            if (field.IsConst)
                return;

            // se è `public` o `protected` lasciamo passare
            if (field.DeclaredAccessibility.HasFlag(Accessibility.Public) || field.DeclaredAccessibility.HasFlag(Accessibility.Protected))
                return;

            // se rispetta le convenzioni lasciamo passare
            if (CaseConversions.IsUnderscoreCamelCase(field.Name))
                return;

            var location = field.Locations[0];
            var diagnostic = Diagnostic.Create(Rule, location, field.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
