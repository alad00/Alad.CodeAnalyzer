// SPDX-FileCopyrightText: Microsoft
// SPDX-FileCopyrightText: .NET Foundation and Contributors
// SPDX-FileCopyrightText: 2022 ALAD SRL
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Alad.CodeAnalyzer.Internal
{
    public class WellKnownTypeProvider
    {
        // https://github.com/dotnet/roslyn-analyzers/blob/a497be39fc800a3436287e9e0f1987c9284ed27d/src/Utilities/Compiler/WellKnownTypeProvider.cs

        class CompilationCache
        {
            public CompilationCache(Compilation compilation)
            {
                Compilation = compilation;

                _referencedAssemblies = new Lazy<ImmutableHashSet<IAssemblySymbol>>(
                    () => ImmutableHashSet.Create<IAssemblySymbol>(SymbolEqualityComparer.Default, Compilation.Assembly.Modules.SelectMany(m => m.ReferencedAssemblySymbols).ToArray()),
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public Compilation Compilation { get; }

            public ConcurrentDictionary<string, INamedTypeSymbol> FullNameToType { get; }
                = new ConcurrentDictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

            public ConcurrentDictionary<string, ImmutableArray<string>> FullNameToNamespace { get; }
                = new ConcurrentDictionary<string, ImmutableArray<string>>(StringComparer.Ordinal);

            readonly Lazy<ImmutableHashSet<IAssemblySymbol>> _referencedAssemblies;
            public ImmutableHashSet<IAssemblySymbol> ReferencedAssemblies => _referencedAssemblies.Value;
        }

        static readonly ConditionalWeakTable<Compilation, CompilationCache> s_cache
            = new ConditionalWeakTable<Compilation, CompilationCache>();

        readonly CompilationCache _cache;

        public WellKnownTypeProvider(Compilation compilation)
        {
            _cache = s_cache.GetValue(compilation, c => new CompilationCache(c));
        }

        public Compilation Compilation => _cache.Compilation;

        public bool TryGetOrCreateTypeByMetadataName(string fullTypeName, out INamedTypeSymbol namedTypeSymbol)
        {
            if (_cache.FullNameToType.TryGetValue(fullTypeName, out namedTypeSymbol))
                return namedTypeSymbol != null;

            return TryGetOrCreateTypeByMetadataNameSlow(fullTypeName, out namedTypeSymbol);
        }

        bool TryGetOrCreateTypeByMetadataNameSlow(string fullTypeName, out INamedTypeSymbol namedTypeSymbol)
        {
            namedTypeSymbol = _cache.FullNameToType.GetOrAdd(
                fullTypeName,
                fullyQualifiedMetadataName =>
                {
                    // Caching null results is intended.

                    // sharwell says: Suppose you reference assembly A with public API X.Y, and you reference assembly B with
                    // internal API X.Y. Even though you can use X.Y from assembly A, compilation.GetTypeByMetadataName will
                    // fail outright because it finds two types with the same name.

                    INamedTypeSymbol type = null;

                    ImmutableArray<string> namespaceNames;
#if NETSTANDARD1_3 // Probably in 2.9.x branch; just don't cache.
                    namespaceNames = GetNamespaceNamesFromFullTypeName(fullTypeName);
#else // Assuming we're on .NET Standard 2.0 or later, cache the type names that are probably compile time constants.
                    if (string.IsInterned(fullTypeName) != null)
                    {
                        namespaceNames = _cache.FullNameToNamespace.GetOrAdd(
                            fullTypeName,
                            GetNamespaceNamesFromFullTypeName);
                    }
                    else
                    {
                        namespaceNames = GetNamespaceNamesFromFullTypeName(fullTypeName);
                    }
#endif

                    if (IsSubsetOfCollection(namespaceNames, Compilation.Assembly.NamespaceNames))
                    {
                        type = Compilation.Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName);
                    }

                    if (type is null)
                    {
                        Debug.Assert(namespaceNames != null);

                        foreach (IAssemblySymbol referencedAssembly in _cache.ReferencedAssemblies)
                        {
                            if (!IsSubsetOfCollection(namespaceNames, referencedAssembly.NamespaceNames))
                            {
                                continue;
                            }

                            var currentType = referencedAssembly.GetTypeByMetadataName(fullyQualifiedMetadataName);
                            if (currentType is null)
                            {
                                continue;
                            }

                            switch (currentType.GetResultantVisibility())
                            {
                                case SymbolVisibility.Public:
                                case SymbolVisibility.Internal when referencedAssembly.GivesAccessTo(Compilation.Assembly):
                                    break;

                                default:
                                    continue;
                            }

                            if (type is object)
                            {
                                // Multiple visible types with the same metadata name are present.
                                return null;
                            }

                            type = currentType;
                        }
                    }

                    return type;
                });

            return namedTypeSymbol != null;
        }

        static ImmutableArray<string> GetNamespaceNamesFromFullTypeName(string fullTypeName)
        {
            var namespaceNamesBuilder = ImmutableArray.CreateBuilder<string>();

            Debug.Assert(namespaceNamesBuilder != null);

            int prevStartIndex = 0;
            for (int i = 0; i < fullTypeName.Length; i++)
            {
                if (fullTypeName[i] == '.')
                {
                    namespaceNamesBuilder.Add(fullTypeName.Substring(prevStartIndex, i - prevStartIndex));
                    prevStartIndex = i + 1;
                }
                else if (!IsIdentifierPartCharacter(fullTypeName[i]))
                {
                    break;
                }
            }

            return namespaceNamesBuilder.ToImmutable();
        }

        static bool IsIdentifierPartCharacter(char ch)
        {
            // identifier-part-character:
            //   letter-character
            //   decimal-digit-character
            //   connecting-character
            //   combining-character
            //   formatting-character

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return ch >= '0'  // '\u0030'
                        && ch <= '9'; // '\u0039'
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_'; // '\u005F'
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);

            ////return IsLetterChar(cat)
            ////    || IsDecimalDigitChar(cat)
            ////    || IsConnectingChar(cat)
            ////    || IsCombiningChar(cat)
            ////    || IsFormattingChar(cat);

            switch (cat)
            {
                // Letter
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.Format:
                    return true;
                default:
                    return false;
            };
        }

        static bool IsSubsetOfCollection<T>(ImmutableArray<T> set1, ICollection<T> set2)
        {
            if (set1.Length > set2.Count)
            {
                return false;
            }

            for (int i = 0; i < set1.Length; i++)
            {
                if (!set2.Contains(set1[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
