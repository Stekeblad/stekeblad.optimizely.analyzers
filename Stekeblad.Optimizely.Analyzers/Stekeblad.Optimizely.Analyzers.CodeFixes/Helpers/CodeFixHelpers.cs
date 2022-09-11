using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.Helpers
{
	public static class CodeFixHelpers
	{
		/// <summary>
		/// Adds a new attribute with option to include a list of parameters
		/// </summary>
		/// <param name="document">The document the object to update is inside</param>
		/// <param name="compilationUnitSyntax">The compilation unit / syntax root of the document</param>
		/// <param name="memberOrType">The member or type to add the attribute to</param>
		/// <param name="attributeName">Name of the new attribute</param>
		/// <param name="attributeParamters">Atribute parameters as a normal string, example: <c>@"GroupName = ""Content"""</c></param>
		/// <returns></returns>
		public static Document AddAttributeDeclaration(ref Document document,
			ref CompilationUnitSyntax compilationUnitSyntax,
			MemberDeclarationSyntax memberOrType,
			string attributeName,
			string attributeParamters = null)
		{
			// Create the syntax for the new Attribute
			var attributeNameSyntax = SyntaxFactory.ParseName(attributeName);
			AttributeSyntax attribute;

			// Add parameters, if provided
			if (attributeParamters == null)
			{
				attribute = SyntaxFactory.Attribute(attributeNameSyntax);
			}
			else
			{
				var attributeArguments = SyntaxFactory.ParseAttributeArgumentList($"({attributeParamters})");
				attribute = SyntaxFactory.Attribute(attributeNameSyntax, attributeArguments);
			}

			// Attributes must belong to a "AttributeList"
			// The thing that allows to have multiple attribute separated by comma, like [required, MaxLength(8)]
			var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));

			// Add the attributeList to the type declaration, update the compilationUnit node and document
			var newTypeDecl = memberOrType.AddAttributeLists(attributeList)
				.WithAdditionalAnnotations(Formatter.Annotation);
			compilationUnitSyntax = compilationUnitSyntax.ReplaceNode(memberOrType, newTypeDecl);
			document = document.WithSyntaxRoot(compilationUnitSyntax);

			return document;
		}

		/// <summary>
		/// Adds a using directive with the namespace in <c>usingNamespace</c> if it does not already exists in the document.
		/// This method takes both the document and the CompilationUnitSyntax by reference meaning that the passed
		/// variables has been updated when the method returns.
		/// </summary>
		/// <param name="document">The document to add the using to</param>
		/// <param name="compilationUnitSyntax">The compilation unit / syntax root of the document</param>
		/// <param name="usingNamespace">The namespace to add a using for</param>
		public static void AddUsingIfMissing(ref Document document, ref CompilationUnitSyntax compilationUnitSyntax, string usingNamespace)
		{
			if (!compilationUnitSyntax.Usings.Any(u => u.Name.ToString().Equals(usingNamespace)))
			{
				compilationUnitSyntax = compilationUnitSyntax.AddUsings(
					SyntaxFactory.UsingDirective(
						SyntaxFactory.ParseName(usingNamespace)));

				document = document.WithSyntaxRoot(compilationUnitSyntax);
			}
		}
	}
}
