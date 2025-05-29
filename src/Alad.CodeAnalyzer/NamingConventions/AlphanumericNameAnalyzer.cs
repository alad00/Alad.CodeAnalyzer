// SPDX-FileCopyrightText: 2025 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Alad.CodeAnalyzer.NamingConventions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AlphanumericNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.NameSpecialCharacters,
            title: "Utilizzare solo lettere, numeri, o underscore nei nomi",
            messageFormat: "Rinominare '{0}' per usare solo lettere, numeri, o underscore",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.NameSpecialCharacters}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: dovremmo gestire anche SymbolKind.Alias, SymbolKind.ArrayType, SymbolKind.Assembly, SymbolKind.DynamicType, SymbolKind.Label, SymbolKind.Local, SymbolKind.PointerType, SymbolKind.RangeVariable, SymbolKind.TypeParameter, SymbolKind.Preprocessing, SymbolKind.FunctionPointerType.
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.NamedType, SymbolKind.Namespace, SymbolKind.Parameter, SymbolKind.Property);
            context.RegisterSyntaxNodeAction(AnalyzeVariable, SyntaxKind.VariableDeclarator);
        }

        static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // Niente diagnostiche sui valori generati dal compilatore.
            if (context.Symbol.IsImplicitlyDeclared)
                return;

            // Niente diagnostiche sui costruttori, dato che già è stato segnalato sul nome della classe.
            if (context.Symbol is IMethodSymbol method && (method.MethodKind is MethodKind.Constructor || method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet))
                return;

            bool isNormalized = IsNormalized(context.Symbol.Name);
            if (isNormalized)
                return;

            var parameters = ImmutableDictionary.CreateBuilder<string, string>();
            parameters.Add("name", context.Symbol.Name);

            var location = context.Symbol.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, parameters.ToImmutable(), context.Symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private void AnalyzeVariable(SyntaxNodeAnalysisContext context)
        {
            VariableDeclaratorSyntax variable = (VariableDeclaratorSyntax)context.Node;
            string name = variable.Identifier.Text;

            bool isNormalized = IsNormalized(variable.Identifier.Text);
            if (isNormalized)
                return;

            var parameters = ImmutableDictionary.CreateBuilder<string, string>();
            parameters.Add("name", variable.Identifier.Text);

            var location = variable.Identifier.GetLocation();
            var diagnostic = Diagnostic.Create(s_rule, location, parameters.ToImmutable(), name);
            context.ReportDiagnostic(diagnostic);
        }

        static bool IsNormalized(string name)
            => name.All(c => c is '_' || (c <= (char)127 && char.IsLetterOrDigit(c)));
    }
}
