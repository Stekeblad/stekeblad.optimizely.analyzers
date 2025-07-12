using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	public static class MemberDeclarationSyntaxExtensions
	{
		/// <summary>
		/// Returns the syntax of the <c>abstract</c> modifier (keyword) for the provided member syntax.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">
		///		If the given member syntax does not have the abstract modifier
		/// </exception>
		public static SyntaxToken GetAbstractModifier(this MemberDeclarationSyntax memberDeclaration)
		{
			return memberDeclaration.Modifiers.First(x => x.IsKind(SyntaxKind.AbstractKeyword));
		}
	}
}
