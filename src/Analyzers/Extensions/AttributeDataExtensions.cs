using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class AttributeDataExtensions
	{
		/// <summary>
		/// Tries to find a "NamedArgument" called <c>argumentName</c> on the attribute
		/// in source referenced by <c>attribute</c>. If an argument with the given name
		/// is found it's returned in the <c>argument</c> parameter and the method returns
		/// true. If an argument is not found the method returns false.
		/// </summary>
		/// <param name="attribute">The attribute to find an argument on</param>
		/// <param name="argumentName">name of the argument to find</param>
		/// <param name="argument">out parameter with the argument, if found</param>
		/// <returns>true if the argumemt was found, false otherwise</returns>
		public static bool TryGetArgument(this AttributeData attribute, string argumentName, out TypedConstant argument)
		{
			for (int i = 0; i < attribute.NamedArguments.Length; i++)
			{
				KeyValuePair<string, TypedConstant> na = attribute.NamedArguments[i];
				if (na.Key.Equals(argumentName, StringComparison.Ordinal))
				{
					argument = na.Value;
					return true;
				}
			}

			// Note to self if I want to support naming constructor parameters in the future:
			// INamedTypeSymbol.Constructors[0].Parameters[0].Name

			argument = default;
			return false;
		}

		public static Location GetLocation(this AttributeData attribute)
		{
			return attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
		}
	}
}
