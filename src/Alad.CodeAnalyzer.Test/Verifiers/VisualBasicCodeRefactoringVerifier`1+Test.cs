﻿// SPDX-FileCopyrightText: Microsoft
// SPDX-FileCopyrightText: .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2022 ALAD SRL
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Alad.CodeAnalyzer.Test;

public static partial class VisualBasicCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    public class Test : VisualBasicCodeRefactoringTest<TCodeRefactoring, MSTestVerifier>
    {
    }
}