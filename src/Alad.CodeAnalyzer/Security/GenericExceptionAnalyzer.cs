// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.Security
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GenericExceptionAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Security.GenericException,
            title: "Usare un tipo più specifico di eccezione",
            messageFormat: "{0} non specializzata",
            category: nameof(AladDiagnosticCodes.Security),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/blob/main/docs/codes/{AladDiagnosticCodes.Security.GenericException}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(Analyze, OperationKind.ObjectCreation);
        }

        static void Analyze(OperationAnalysisContext context)
        {
            var operation = (IObjectCreationOperation)context.Operation;
            var syntax = (ObjectCreationExpressionSyntax)operation.Syntax;

            // se non è un `throw new Exception()` lasciamo passare
            if (
                operation.Type.ContainingNamespace.Name != nameof(System) ||
                operation.Type.Name != nameof(Exception)
            ) return;

            var location = syntax.Type.GetLocation();
            var diagnostic = Diagnostic.Create(s_rule, location, operation.Type.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
