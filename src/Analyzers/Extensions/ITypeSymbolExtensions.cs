using Microsoft.CodeAnalysis;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class ITypeSymbolExtensions
	{
		/// <summary>
		/// Like <c>if (aClass is MyClass)</c> but for two instances of ITypeSymbol
		/// </summary>
		public static bool IsDerivedFrom(this ITypeSymbol analyzedSymbol, ITypeSymbol baseTypeSymbol)
		{
			if (analyzedSymbol?.SpecialType == SpecialType.None)
			{
				// Sealed types can not be inherited, avoid checking inheritance chain
				if (baseTypeSymbol.IsSealed)
					return analyzedSymbol.EqualsSymbol(baseTypeSymbol);

				do
				{
					if (analyzedSymbol.EqualsSymbol(baseTypeSymbol))
						return true;

					analyzedSymbol = analyzedSymbol.BaseType;
				} while (analyzedSymbol?.SpecialType == SpecialType.None);
			}

			return false;
		}
	}
}
