// SPDX-FileCopyrightText: 2023 ALAD SRL <info@alad.cloud>
//
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Alad.CodeAnalyzer.Internal.Tasks
{
    public class AsyncTypeSymbols
    {
        readonly ConditionalWeakTable<ITypeSymbol, StrongBox<bool>> _isAwaitable = new ConditionalWeakTable<ITypeSymbol, StrongBox<bool>>();

        public AsyncTypeSymbols(
            INamedTypeSymbol valueTaskType,
            INamedTypeSymbol valueTask1Type,
            INamedTypeSymbol asyncResultType,
            INamedTypeSymbol awaitExpectedAttributeType,
            INamedTypeSymbol dbContextType,
            INamedTypeSymbol entityFrameworkQueryableExtensionsType)
        {
            ValueTask = valueTaskType;
            ValueTask1 = valueTask1Type;
            IAsyncResult = asyncResultType;
            ExpectsAwaitAttribute = awaitExpectedAttributeType;
            DbContext = dbContextType;
            EntityFrameworkQueryableExtensions = entityFrameworkQueryableExtensionsType;
        }

        public INamedTypeSymbol ValueTask { get; }

        public INamedTypeSymbol ValueTask1 { get; }

        public INamedTypeSymbol IAsyncResult { get; }

        public INamedTypeSymbol ExpectsAwaitAttribute { get; }

        public INamedTypeSymbol DbContext { get; }

        public INamedTypeSymbol EntityFrameworkQueryableExtensions { get; }

        public bool IsAwaitableType(ITypeSymbol type)
        {
            return _isAwaitable.GetValue(type, t =>
            {
                // se è ValueTask o ValueTask<T> è awaitable
                if (t.Equals(ValueTask, SymbolEqualityComparer.Default) ||
                    t.OriginalDefinition.Equals(ValueTask1, SymbolEqualityComparer.Default))
                    return new StrongBox<bool>(true);

                if (IAsyncResult is null)
                    return new StrongBox<bool>(false); // non dovrebbe mai poter succedere

                // se implementa IAsyncResult è awaitable
                bool isAwaitable = t.AllInterfaces.Any(i => i.Equals(IAsyncResult, SymbolEqualityComparer.Default));
                return new StrongBox<bool>(isAwaitable);
            }).Value;
        }

        public bool IsAwaitableMethod(IMethodSymbol method)
        {
            return IsAwaitableType(method.ReturnType);
        }

        public bool IsImmediatelyAwaited(IInvocationOperation operation)
        {
            // await diretto
            if (operation.Parent is IAwaitOperation)
                return true;

            // ci sono vari altri modi per fare await senza usare l'operatore await,
            // non è sempre vero, ma per tenerla semplice si considera un accesso a membro come equivalente ad await
            if (operation.Parent is IMemberReferenceOperation)
                return true;

            // return diretto è un'alternativa a fare await
            if (operation.Parent is IReturnOperation)
                return true;

            return false;
        }

        public bool ExpectsAwait(IMethodSymbol method)
        {
            // i metodi di Entity Framework Core devono essere "awaited"
            if (method.ContainingType.Equals(EntityFrameworkQueryableExtensions, SymbolEqualityComparer.Default) ||
                method.ContainingType.ExtendsType(DbContext))
                return true;

            if (ExpectsAwaitAttribute is null)
                return false;

            return method.GetAttributes()
                .Where(a => a.AttributeClass.Equals(ExpectsAwaitAttribute, SymbolEqualityComparer.Default))
                .Any(a => Equals(a.ConstructorArguments[0].Value, true));
        }
    }

    public static class AnalysisContextAsyncExtensions
    {
        static readonly ConditionalWeakTable<WellKnownTypeProvider, AsyncTypeSymbols> s_symbols = new ConditionalWeakTable<WellKnownTypeProvider, AsyncTypeSymbols>();

        public static AsyncTypeSymbols GetAsyncTypeSymbols(this WellKnownTypeProvider typeProvider)
        {
            return s_symbols.GetValue(typeProvider, tp =>
            {
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask, out var valueTask);
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask1, out var valueTask1);
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIAsyncResult, out var asyncResult);
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.AladDiagnosticsCodeAnalysisExpectsAwaitAttribute, out var expectsAwaitAttribute);
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftEntityFrameworkCoreDbContext, out var dbContext);
                _ = tp.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftEntityFrameworkCoreEntityFrameworkQueryableExtensions, out var entityFrameworkQueryableExtensions);

                return new AsyncTypeSymbols(
                    valueTask,
                    valueTask1,
                    asyncResult,
                    expectsAwaitAttribute,
                    dbContext,
                    entityFrameworkQueryableExtensions);
            });
        }
    }
}
