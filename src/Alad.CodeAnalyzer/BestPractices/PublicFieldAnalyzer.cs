// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.BestPractices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicFieldAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.BestPractices.PublicField,
            title: "I field non dovrebbero mai essere public o protected",
            messageFormat: "Field '{0}' con modificatore di visibilità '{1}'",
            category: nameof(AladDiagnosticCodes.BestPractices),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.BestPractices.PublicField}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
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

            // se non è `public` o `protected` lasciamo passare
            if (!field.DeclaredAccessibility.HasFlag(Accessibility.Public) && !field.DeclaredAccessibility.HasFlag(Accessibility.Protected))
                return;

            var location = field.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, field.Name, field.DeclaredAccessibility.GetCSharpName());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
