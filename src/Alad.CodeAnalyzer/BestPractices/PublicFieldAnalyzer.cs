using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.BestPractices
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PublicFieldAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.BestPractices.PublicField,
            title: "I field non dovrebbero mai essere public o protected",
            messageFormat: "Field '{0}' con modificatore di visibilità '{1}'",
            category: nameof(AladDiagnosticCodes.BestPractices),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/tree/master/docs/codes/{AladDiagnosticCodes.BestPractices.PublicField}.md");

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

            // se non è `public` o `protected` lasciamo passare
            if (!field.DeclaredAccessibility.HasFlag(Accessibility.Public) && !field.DeclaredAccessibility.HasFlag(Accessibility.Protected))
                return;

            var location = field.Locations[0];
            var diagnostic = Diagnostic.Create(Rule, location, field.Name, AccessibilityToCSharp(field.DeclaredAccessibility));
            context.ReportDiagnostic(diagnostic);
        }

        static string AccessibilityToCSharp(Accessibility accessibility)
        {
            if (accessibility.HasFlag(Accessibility.Public))
                return "public";

            string s = null;

            if (accessibility.HasFlag(Accessibility.Protected))
                s = (s == null ? "" : $"{s} ") + "protected";

            if (accessibility.HasFlag(Accessibility.Internal))
                s = (s == null ? "" : $"{s} ") + "internal";

            if (s == null)
                return "private";

            return s;
        }
    }
}
