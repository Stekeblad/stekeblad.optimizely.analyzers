using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class INamedTypeSymbolExtensions
	{
		public static bool IsDerivedFrom(this INamedTypeSymbol analyzedSymbol, INamedTypeSymbol baseTypeSymbol)
		{
			while (analyzedSymbol?.SpecialType == SpecialType.None)
			{
				if (SymbolEqualityComparer.Default.Equals(analyzedSymbol, baseTypeSymbol))
					return true;

				analyzedSymbol = analyzedSymbol.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Checks if <c>analyzedSymbol</c> is decorated with the attribute <c>attributeToFind</c>
		/// or any attribute deriving from <c>attributeToFind</c>. Base types is NOT
		/// examined when searching for attributes.
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <c>attributeToFind</c></param>
		/// <param name="attributeToFind">The attribute to test for on <c>analyzedSymbol</c></param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool TryGetAttributeOrDerivedAttribute(this INamedTypeSymbol analyzedSymbol,
			INamedTypeSymbol attributeToFind,
			out AttributeData foundAttributeData)
		{
			ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();

			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i].AttributeClass.IsDerivedFrom(attributeToFind))
				{
					foundAttributeData = attributes[i];
					return true;
				}
			}
			foundAttributeData = null;
			return false;
		}
	}
}
