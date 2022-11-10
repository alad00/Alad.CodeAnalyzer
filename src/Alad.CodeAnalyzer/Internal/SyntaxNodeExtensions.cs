using Microsoft.CodeAnalysis;

namespace Alad.CodeAnalyzer.Internal
{
    public static class SyntaxNodeExtensions
    {
        public static T FindParent<T>(this SyntaxNode node) where T : SyntaxNode
        {
            SyntaxNode candidate = node;
            do
            {
                if (candidate is T match)
                    return match;
            }
            while ((candidate = candidate.Parent) != null);

            return null;
        }
    }
}
