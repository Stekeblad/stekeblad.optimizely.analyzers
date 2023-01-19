using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class ISymbolExtensions
	{
		/// <summary>
		/// Checks if <c>analyzedSymbol</c> is decorated with the attribute <c>attributeToFind</c>
		/// or any attribute deriving from <c>attributeToFind</c>. Base types of <c>analyzedSymbol</c> is NOT
		/// examined when searching for attributes.
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <c>attributeToFind</c></param>
		/// <param name="attributeToFind">The attribute to test for on <c>analyzedSymbol</c></param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool HasAttributeDerivedFrom(this ISymbol analyzedSymbol, INamedTypeSymbol attributeToFind)
		{
			ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();

			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i].AttributeClass.IsDerivedFrom(attributeToFind))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if <c>analyzedSymbol</c> is decorated with the attribute <c>attributeToFind</c>
		/// or any attribute deriving from <c>attributeToFind</c>. Base types of <c>analyzedSymbol</c> is NOT
		/// examined when searching for attributes.
		/// To not get derived attributes, use <see cref="TryGetAttribute(ISymbol, INamedTypeSymbol, out AttributeData)" />
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <c>attributeToFind</c></param>
		/// <param name="attributeToFind">The attribute to test for on <c>analyzedSymbol</c></param>
		/// <param name="foundAttributeData">Will be set to the first found matching attribute, or null if
		/// no matching attribute was found.</param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool TryGetAttributeDerivedFrom(this ISymbol analyzedSymbol,
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

		/// <summary>
		/// Checks if <c>analyzedSymbol</c> is decorated with the attribute <c>attributeToFind</c>
		/// If you want to include derived attributes aswell,
		/// use <see cref="TryGetAttributeDerivedFrom(ISymbol, INamedTypeSymbol, out AttributeData)" />
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <c>attributeToFind</c></param>
		/// <param name="attributeToFind">The attribute to test for on <c>analyzedSymbol</c></param>
		/// <param name="foundAttributeData">Will be set to the first found matching attribute, or null if
		/// no matching attribute was found.</param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool TryGetAttribute(this ISymbol analyzedSymbol,
			INamedTypeSymbol attributeToFind,
			out AttributeData foundAttributeData)
		{
			ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();

			for (int i = 0; i < attributes.Length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(attributeToFind, attributes[i].AttributeClass))
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
