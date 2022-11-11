// SPDX-FileCopyrightText: 2022 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Alad.CodeAnalyzer.Internal.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Alad.CodeAnalyzer.Logging
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DecorativeLogAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: AladDiagnosticCodes.Logging.DecorativeLog,
            title: "Non è necessario aggiungere elementi ornamentali ai log",
            messageFormat: "Rimuovere elementi ornamentali dal log",
            category: nameof(AladDiagnosticCodes.Logging),
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: $"https://github.com/alad00/Alad.CodeAnalyzer/tree/master/docs/codes/{AladDiagnosticCodes.Logging.DecorativeLog}.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationLogInvocationAction(Analyze);
        }

        static void Analyze(OperationAnalysisContext context, LogNamedTypeSymbols logSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;

            var message = logSymbols.GetMessageArgument(invocation);

            // se non ha un message, non è un metodo di log rilevante
            if (message == null)
                return;

            // se il message non ha valore costante non è possibile analizzarlo
            var value = message.Value.ConstantValue;
            if (!value.HasValue || !(value.Value is string str))
                return;

            // segnalazione se il message è vuoto oppure se inizia o finisce con un carattere ornamentale (simboli o spazi)
            if (str.Length == 0 || IsDecorative(str[0]) || IsDecorative(str[str.Length - 1], true))
            {
                var location = message.Syntax.GetLocation();
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }

        static bool IsDecorative(char chr, bool allowPunctuation = false)
        {
            if (char.IsWhiteSpace(chr))
                return true;

            if (char.IsSymbol(chr))
                return true;

            if (!allowPunctuation && char.IsPunctuation(chr))
                return true;

            return false;
        }
    }
}
