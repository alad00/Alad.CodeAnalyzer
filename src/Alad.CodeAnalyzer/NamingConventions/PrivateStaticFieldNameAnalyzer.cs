// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.NamingConventions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrivateStaticFieldNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName,
            title: "Il nome dei field static internal o private deve essere in formato 's_camelCase' oppure 't_camelCase' se thread-static",
            messageFormat: "Rinominare il field '{0}' per rispettare le convenzioni",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.PrivateStaticFieldName}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(context2 =>
            {
                var wellKnownTypeProvider = new WellKnownTypeProvider(context2.Compilation);

                if (!wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadStaticAttribute, out var threadStaticAttributeType))
                {
                    return;
                }

                context2.RegisterSymbolAction(c => Analyze(threadStaticAttributeType, c), SymbolKind.Field);
            });
        }

        static void Analyze(INamedTypeSymbol threadStaticAttributeType, SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;

            // se non è statico lasciamo passare
            if (!field.IsStatic)
                return;

            // se è `public` o `protected` lasciamo passare
            if (field.DeclaredAccessibility.HasFlag(Accessibility.Public) || field.DeclaredAccessibility.HasFlag(Accessibility.Protected))
                return;

            // se è thread-static il prefisso è diverso
            var isThreadStatic = field.GetAttributes().Any(a => a.AttributeClass.Equals(threadStaticAttributeType, SymbolEqualityComparer.Default));

            // se rispetta le convenzioni lasciamo passare
            if (isThreadStatic ? CaseConversions.IsThreadStaticFieldCase(field.Name) : CaseConversions.IsStaticFieldCase(field.Name))
                return;

            var location = field.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, field.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
