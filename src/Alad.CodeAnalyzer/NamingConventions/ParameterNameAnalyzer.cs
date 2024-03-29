﻿// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
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
    public class ParameterNameAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.NamingConventions.ParameterName,
            title: "Il nome dei parametri deve essere in formato 'camelCase'",
            messageFormat: "Rinominare il parametro '{0}' per rispettare le convenzioni",
            category: nameof(AladDiagnosticCodes.NamingConventions),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.NamingConventions.ParameterName}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(Analyze, SymbolKind.Parameter);
        }

        static void Analyze(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            // se rispetta le convenzioni lasciamo passare
            if (CaseConversions.IsCamelCase(parameter.Name))
                return;

            // se è un parametro del costruttore principale di un `record` lasciamo passare
            if (IsRecordConstructorParameter(parameter))
                return;

            // se è un override ed il metodo sottostante ha un parametro con lo stesso nome, lasciamo passare
            // idem se è l'implementazione del metodo di un'interfaccia ed ha un parametro con lo stesso nome
            if (IsMethodOverrideWithParameterWithSameName(parameter))
                return;

            var location = parameter.Locations[0];
            var diagnostic = Diagnostic.Create(s_rule, location, parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }

        static bool IsRecordConstructorParameter(IParameterSymbol parameter)
        {
            var method = (IMethodSymbol)parameter.ContainingSymbol;
            var isConstructor = method.Name == ".ctor";

            if (!isConstructor)
                return false;

            var type = parameter.ContainingType;
            var hasGeneratedCloneMethod = type.GetMembers("<Clone>$")
                .OfType<IMethodSymbol>()
                .Any(m => m.ReturnType.Equals(type, SymbolEqualityComparer.Default));

            // se ha il metodo generato di clone, è probabilmente un `record`
            if (!hasGeneratedCloneMethod)
                return false;

            return true;
        }

        static bool IsMethodOverrideWithParameterWithSameName(IParameterSymbol parameter)
        {
            var method = (IMethodSymbol)parameter.ContainingSymbol;

            // se è un override ed il metodo sottostante ha un parametro con lo stesso nome, lasciamo passare
            if (method.IsOverride && method.OverriddenMethod.Parameters.Any(p => p.Name == parameter.Name))
                return true;

            // idem se è l'implementazione esplicita del metodo di un'interfaccia ed ha un parametro con lo stesso nome 
            if (method.ExplicitInterfaceImplementations.Any(e => e.Parameters.Any(p => p.Name == parameter.Name)))
                return true;

            // TODO: idem se è l'implementazione implicita del metodo di un'interfaccia ed ha un parametro con lo stesso nome

            return false;
        }
    }
}
