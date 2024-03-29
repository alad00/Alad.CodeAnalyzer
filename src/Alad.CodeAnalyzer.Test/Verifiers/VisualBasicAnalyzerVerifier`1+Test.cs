﻿// SPDX-FileCopyrightText: Microsoft
// SPDX-FileCopyrightText: .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2022 ALAD SRL
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Alad.CodeAnalyzer.Test;

public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public class Test : VisualBasicAnalyzerTest<TAnalyzer, MSTestVerifier>
    {
        public Test()
        {
        }
    }
}