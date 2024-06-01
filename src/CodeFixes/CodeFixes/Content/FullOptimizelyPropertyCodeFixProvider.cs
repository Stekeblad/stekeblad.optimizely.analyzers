using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Stekeblad.Optimizely.Analyzers.Analyzers.Content;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.FullOptimizelyPropertyAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.Content
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FullOptimizelyPropertyAnalyzer)), Shared]
	public class FullOptimizelyPropertyCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(Analyzer.FullOptimizelyPropertyDiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return null; // Don't allow multiple occurrences to be fixed at once
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the property declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent as PropertyDeclarationSyntax;

			CodeAction action = CodeAction.Create(
				title: Analyzer.FullOptimizelyPropertyTitle,
				createChangedDocument: c => ExpandOptimizelyProperty(context.Document, declaration, c),
				equivalenceKey: Analyzer.FullOptimizelyPropertyDiagnosticId);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> ExpandOptimizelyProperty(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
		{
			var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var oldDeclaration = declaration;

			// Expand the property getter and remove the semicolon after "get"
			var getterDeclaration = declaration.AccessorList.Accessors.First(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
			var newGetter = getterDeclaration // get;
				.WithBody(CreateGetterBody(declaration)) // get { /* getter body */ };
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)); // get { /* getter body */ }
			declaration = declaration.ReplaceNode(getterDeclaration, newGetter);

			// Expand the property setter and remove the semicolon after "set"
			var setterDeclaration = declaration.AccessorList.Accessors.First(x => x.IsKind(SyntaxKind.SetAccessorDeclaration));
			var newSetter = setterDeclaration // set;
				.WithBody(CreateSetterBody(declaration)) // set { /* setter body */ };
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)); // set { /* setter body */ }
			declaration = declaration.ReplaceNode(setterDeclaration, newSetter);

			// insert new code and format it (so it's not all an a single line)
			root = root.ReplaceNode(oldDeclaration, declaration.WithAdditionalAnnotations(Formatter.Annotation));
			return document.WithSyntaxRoot(root);
		}

		/// <summary>
		/// Assume the property to fix is named <c>Heading</c>
		/// then following syntax will be generated:
		/// <code>
		/// var heading = this.GetPropertyValue(p => p.Heading);
		/// return heading;
		/// </code>
		/// </summary>
		private BlockSyntax CreateGetterBody(PropertyDeclarationSyntax declaration)
		{
			string propertyName = declaration.Identifier.Text;

			// Creates: this.GetPropertyValue
			var getPropertyValue = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.ThisExpression(),
				SyntaxFactory.IdentifierName("GetPropertyValue"));

			// Creates: (p => p.$propertyName)
			var getPropertyValueArguments = SyntaxFactory.ArgumentList(
				SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.Argument(
						SyntaxFactory.SimpleLambdaExpression(
							SyntaxFactory.Parameter(SyntaxFactory.Identifier("p")),
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								SyntaxFactory.IdentifierName("p"),
								SyntaxFactory.IdentifierName(propertyName)
							)))));

			//Creates: this.GetPropertyValue(p => p.$propertyName)
			var callGetPropertyValue = SyntaxFactory.InvocationExpression(getPropertyValue, getPropertyValueArguments);

			// Convert the first character in the property name to lowercase. If user code follows convention
			// the name will begin with uppercase and the variable will make sense.
			// If it's already lowercase it may look a bit weird but will still be legal and behave.
			string variableName = propertyName;
			variableName = variableName[0].ToString().ToLower() + variableName.Substring(1);

			// Creates: var $variableName = this.GetPropertyValue(p => p.$propertyName);
			var variableDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(
				SyntaxFactory.VariableDeclaration(
					SyntaxFactory.IdentifierName("var"),
					SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.VariableDeclarator(variableName)
						.WithInitializer(SyntaxFactory.EqualsValueClause(callGetPropertyValue))
					)));

			// Creates: return $variableName;
			var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(variableName));

			// Put everything together
			var syntaxList = SyntaxFactory.List<SyntaxNode>()
				.Add(variableDeclarationStatement)
				.Add(returnStatement);
			return SyntaxFactory.Block(syntaxList);
		}

		/// <summary>
		/// Assume the property to fix is named <c>Heading</c>
		/// then following syntax will be generated:
		/// <code>
		/// this.SetPropertyValue(p => p.Heading, value);
		/// </code>
		/// </summary>
		private BlockSyntax CreateSetterBody(PropertyDeclarationSyntax declaration)
		{
			string propertyName = declaration.Identifier.Text;

			// Creates: this.SetPropertyValue
			var setPropertyValue = SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxFactory.ThisExpression(),
				SyntaxFactory.IdentifierName("SetPropertyValue"));

			// Creates: (p => p.$propertyName, value)
			var setPropertyValueArguments = SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList(new List<SyntaxNode>() {
					// p => p.$propertyName
					SyntaxFactory.Argument(
						SyntaxFactory.SimpleLambdaExpression(
							SyntaxFactory.Parameter(SyntaxFactory.Identifier("p")),
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								SyntaxFactory.IdentifierName("p"),
								SyntaxFactory.IdentifierName(propertyName)
							))),
					// value
					SyntaxFactory.Argument(
						SyntaxFactory.IdentifierName("value"))
				}));

			//Creates: this.SetPropertyValue(p => p.$propertyName, value)
			var callSetPropertyValue = SyntaxFactory.ExpressionStatement(
				SyntaxFactory.InvocationExpression(setPropertyValue, setPropertyValueArguments));

			var syntaxList = SyntaxFactory.List<SyntaxNode>()
				.Add(callSetPropertyValue);
			return SyntaxFactory.Block(syntaxList);
		}
	}
}