using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
		/// Checks if <paramref name="analyzedSymbol"/> is decorated with the attribute <paramref name="attributeToFind"/>
		/// or any attribute deriving from <c>attributeToFind</c>. Base types of <c>analyzedSymbol</c> is NOT
		/// examined when searching for attributes.
		/// To not get derived attributes, use <see cref="TryGetAttribute(ISymbol, INamedTypeSymbol, out AttributeData)" />
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <paramref name="attributeToFind"/></param>
		/// <param name="attributeToFind">The attribute to test for</param>
		/// <param name="foundAttribute">Will be set to the first found matching attribute, or null if
		/// no matching attribute was found.</param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool TryGetAttributeDerivedFrom(this ISymbol analyzedSymbol,
			INamedTypeSymbol attributeToFind,
			out AttributeData foundAttribute)
		{
			ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();

			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i].AttributeClass.IsDerivedFrom(attributeToFind))
				{
					foundAttribute = attributes[i];
					return true;
				}
			}

			foundAttribute = null;
			return false;
		}

		/// <summary>
		/// Checks if <paramref name="analyzedSymbol"/> is decorated with the attribute <paramref name="attributeToFind"/>
		/// If you want to include derived attributes aswell,
		/// use <see cref="TryGetAttributeDerivedFrom(ISymbol, INamedTypeSymbol, out AttributeData)" />
		/// </summary>
		/// <param name="analyzedSymbol">The symbol that may be decorated with <paramref name="attributeToFind"/></param>
		/// <param name="attributeToFind">The attribute to test for</param>
		/// <param name="foundAttribute">Will be set to the first found matching attribute, or null if
		/// no matching attribute was found.</param>
		/// <returns>true if the attribute was found on the analyzed symbol, false otherwise</returns>
		public static bool TryGetAttribute(this ISymbol analyzedSymbol,
			INamedTypeSymbol attributeToFind,
			out AttributeData foundAttribute)
		{
			ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();

			for (int i = 0; i < attributes.Length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(attributeToFind, attributes[i].AttributeClass))
				{
					foundAttribute = attributes[i];
					return true;
				}
			}

			foundAttribute = null;
			return false;
		}

        /// <summary>
        /// Checks if <paramref name="analyzedSymbol"/> is decorated with the attribute <paramref name="attributeToFind"/>
        /// or any attribute deriving from it. Base types of <paramref name="analyzedSymbol"/> is NOT
        /// examined when searching for attributes.
        /// To not get derived attributes, use <see cref="TryGetAttributesOfType(ISymbol, INamedTypeSymbol, out List{AttributeData})" />
        /// </summary>
        /// <param name="analyzedSymbol">The symbol that may be decorated with <paramref name="attributeToFind"/> or a deriving attribute</param>
        /// <param name="attributeToFind">The attribute to test for</param>
        /// <param name="foundAttributes">A list of found attributes, or null if no matching attribute was found.</param>
        /// <returns>true if atleast one matching attribute was found on the analyzed symbol, false otherwise</returns>
        public static bool TryGetAttributesDerivedFrom(this ISymbol analyzedSymbol,
            INamedTypeSymbol attributeToFind,
            out List<AttributeData> foundAttributes)
        {
            ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();
            foundAttributes = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i].AttributeClass.IsDerivedFrom(attributeToFind))
                {
                    (foundAttributes ?? (foundAttributes = new List<AttributeData>())).Add(attributes[i]);
                }
            }

            return foundAttributes != null;
        }

        /// <summary>
        /// Checks if <paramref name="analyzedSymbol"/> is decorated with the attribute<paramref name="attributeToFind"/>
        /// If you want to include derived attributes as well,
        /// use <see cref="TryGetAttributeDerivedFrom(ISymbol, INamedTypeSymbol, out AttributeData)" />
        /// </summary>
        /// <param name="analyzedSymbol">The symbol that may be decorated with <paramref name="attributeToFind"/></param>
        /// <param name="attributeToFind">The attribute to test for</param>
        /// <param name="foundAttributes">A list of found attributes, or null if no matching attribute was found.</param>
        /// <returns>true if atleast one matching attribute was found on the analyzed symbol, false otherwise</returns>
        public static bool TryGetAttributesOfType(this ISymbol analyzedSymbol,
            INamedTypeSymbol attributeToFind,
            out List<AttributeData> foundAttributes)
        {
            ImmutableArray<AttributeData> attributes = analyzedSymbol.GetAttributes();
            foundAttributes = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (SymbolEqualityComparer.Default.Equals(attributeToFind, attributes[i].AttributeClass))
                {
                    (foundAttributes ?? (foundAttributes = new List<AttributeData>())).Add(attributes[i]);
                }
            }

            return foundAttributes != null;
        }

        /// <summary>
        /// Test if a symbol is a valid Optimizely content property. Tests include if the symbol is a property,
        /// how it is defined and that it's containing type is derived from EPiServer.Core.ContentData
        /// </summary>
        /// <param name="symbol">Symbol to test</param>
        /// <param name="compilation">The current compilation information, to locate and test presence of Optimizely types</param>
        /// <returns>True if symbol meets all criteria for a content property (except for being virtual, that is covered by SOA1003), false otherwise</returns>
        public static bool IsOptiContentProperty(this ISymbol symbol, Compilation compilation)
		{
			// Make sure the symbol is a property and not a field, method, class etc.
			if (!(symbol is IPropertySymbol prop))
				return false;

			// The property must be inside a class that at some point derives from ContentData
			var contentDataSymbol = compilation.GetTypeByMetadataName(
					"EPiServer.Core.ContentData");

			if (contentDataSymbol == null || !symbol.ContainingType.IsDerivedFrom(contentDataSymbol))
				return false;

			// Inspect the keywords used to define the property
			if (prop.DeclaredAccessibility == Accessibility.Public
				&& !prop.IsImplicitlyDeclared // skip autogen props
				&& !prop.IsStatic // must be instance member
				//&& (prop.IsVirtual || prop.IsOverride) // ensured by SOA1003 ContentPropertyMustBeVirtual
				&& prop.SetMethod != null) // Property must have a setter
			{
				// properties decorated with IgnoreAttribute does not show up in the CMS and should be ignored
				var ignoreAttributeSymbol = compilation.GetTypeByMetadataName(
				"EPiServer.DataAnnotations.IgnoreAttribute");

				// return true if IgnoreAttribute is not present
				return ignoreAttributeSymbol == null || !prop.HasAttributeDerivedFrom(ignoreAttributeSymbol);
			}

			// not a content property
			return false;
		}
	}
}
