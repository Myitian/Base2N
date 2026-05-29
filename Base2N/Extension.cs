using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Base2N;

static class Extension
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull<T>(
            [NotNull] scoped T? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
            where T : allows ref struct
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
        }
    }
}