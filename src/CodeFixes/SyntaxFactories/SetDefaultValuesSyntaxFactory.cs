using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Linq;

namespace Stekeblad.Optimizely.Analyzers.SyntaxFactories
{
	public static class SetDefaultValuesSyntaxFactory
	{
		/// <summary>
		/// Checks if <paramref name="propertyName"/> is assigned in <paramref name="defaultValuesMethod"/>.
		/// If it is, then the assignment is changed to <paramref name="propertyDefaultValue"/>
		/// otherwise a new line is added to assign the value to the property.
		/// Modification are reflected in the ref parameters
		/// </summary>
		public static void AddOrUpdateDefaultValue(ref Document document,
			ref CompilationUnitSyntax root,
			ref MethodDeclarationSyntax defaultValuesMethod,
			string propertyName,
			ExpressionSyntax propertyDefaultValue)
		{
			// Look after an existing assignment to the property inside SetDefaultValues method
			ExpressionStatementSyntax propDefaultValueStatement = null;
			foreach (var statement in defaultValuesMethod.Body.DescendantNodes()
				.OfType<ExpressionStatementSyntax>()
				.Where(ess => ess.Expression is AssignmentExpressionSyntax))
			{
				var aes = (AssignmentExpressionSyntax)statement.Expression;
				// We are looking at an assignment, make sure it is assigning to the property we are applying a fix for
				if (aes.Left is IdentifierNameSyntax ins && ins.Identifier.Text.Equals(propertyName))
				{
					propDefaultValueStatement = statement;
					break;
				}
			}

			// Add an assignment expression for the property inside the SetDefaultValues method if one does not exist
			// or update the existing assignment to assign the value the property is initialized with
			if (propDefaultValueStatement == null)
			{
				// Add new assignment
				var assignmentStatement = SyntaxFactory.ExpressionStatement(
					SyntaxFactory.AssignmentExpression(
						SyntaxKind.SimpleAssignmentExpression,
						SyntaxFactory.IdentifierName(propertyName),
						propertyDefaultValue));

				var newDefValuesMethod = defaultValuesMethod.AddBodyStatements(assignmentStatement);
				root = root.ReplaceNode(defaultValuesMethod, newDefValuesMethod);
			}
			else
			{
				// Update existing assignment
				var assignmentExpression = propDefaultValueStatement.Expression as AssignmentExpressionSyntax;
				var newAssignmentExpression = assignmentExpression.WithRight(propertyDefaultValue);
				root = root.ReplaceNode(assignmentExpression, newAssignmentExpression);
			}

			// Update the document to use the new root
			document = document.WithSyntaxRoot(root);
		}

		/// <summary>
		/// Checks <paramref name="containingClass"/> for a method named SetDefaultValues.
		/// If it's found it's returned without modifications.
		/// If it's not found it generates the Optimizely Content Type SetDefaultValues override method
		/// and inserts it into the given class, document and compilation.
		/// </summary>
		public static MethodDeclarationSyntax FindOrCreate(ref Document document, ref CompilationUnitSyntax root, ClassDeclarationSyntax containingClass)
		{
			// Test if class contains a method named SetDefaultValues
			var defaultValuesMethod = containingClass.Members.OfType<MethodDeclarationSyntax>()
				.FirstOrDefault(method => method.Identifier.Text.Equals("SetDefaultValues"));

			// if method is not found, create it and add it to the document root
			if (defaultValuesMethod == null)
			{
				defaultValuesMethod = CreateDefaultValuesMethod();
				var newMembers = containingClass.Members.Add(defaultValuesMethod);
				var newContainingClass = containingClass.WithMembers(newMembers);
				root = root.ReplaceNode(containingClass, newContainingClass);
				CodeFixHelpers.AddUsingIfMissing(ref document, ref root, "EPiServer.DataAbstraction");

				// Syntax tree has changed, locate the containing class and the new SetDefaultValues method
				containingClass = root.DescendantNodes()
					.OfType<ClassDeclarationSyntax>()
					.First(c => c.Identifier.Text.Equals(containingClass.Identifier.Text));

				defaultValuesMethod = containingClass.Members.OfType<MethodDeclarationSyntax>()
					.FirstOrDefault(method => method.Identifier.Text.Equals("SetDefaultValues"));
			}

			return defaultValuesMethod;
		}

		private static MethodDeclarationSyntax CreateDefaultValuesMethod()
		{
			// This method creates the following code:
			// public override void SetDefaultValues(ContentType contentType) { <result of CreateDefaultValuesMethod_CallBaseMethod> }

			var keywords = SyntaxFactory.TokenList(
				SyntaxFactory.Token(SyntaxKind.PublicKeyword),
				SyntaxFactory.Token(SyntaxKind.OverrideKeyword));

			var contentTypeParameter = SyntaxFactory.Parameter(
				SyntaxFactory.Identifier("contentType"))
					.WithType(SyntaxFactory.ParseTypeName("ContentType"));

			var parameterList = SyntaxFactory.ParameterList(
				SyntaxFactory.SingletonSeparatedList(contentTypeParameter));

			var baseMethodCallSyntax = CreateDefaultValuesMethod_CallBaseMethod();

			var newMethodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "SetDefaultValues")
				.WithModifiers(keywords)
				.WithParameterList(parameterList)
				.WithBody(SyntaxFactory.Block(baseMethodCallSyntax));

			return newMethodDeclaration;
		}

		private static ExpressionStatementSyntax CreateDefaultValuesMethod_CallBaseMethod()
		{
			// This method creates the following code:
			// base.SetDefaultValues(contentType);

			var baseMethodSetDefaultValues = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.BaseExpression(),
				SyntaxFactory.IdentifierName("SetDefaultValues"));

			var arguments = SyntaxFactory.ArgumentList(
				SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.Argument(
						 SyntaxFactory.IdentifierName("contentType"))));

			var callBaseMethodStatement = SyntaxFactory.ExpressionStatement(
				SyntaxFactory.InvocationExpression(baseMethodSetDefaultValues, arguments));

			return callBaseMethodStatement;
		}
	}
}
