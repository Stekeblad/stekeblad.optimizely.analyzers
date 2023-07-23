using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.UseSetDefaultValuesAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.Content
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetDefaultValuesCodeFixProvider)), Shared]
	public class UseSetDefaultValuesCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.DiagnosticID); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return null;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the property declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent.Parent as PropertyDeclarationSyntax;

			// Register a code action that will invoke the fix.
			CodeAction action = CodeAction.Create(
				title: Analyzer.Title,
				createChangedDocument: cancellationToken => PerformFix(context.Document, declaration, cancellationToken),
				equivalenceKey: Analyzer.DiagnosticID);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> PerformFix(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
		{
			//Take note of the class the property is declared in.
			var containingClassName = declaration.Ancestors().OfType<ClassDeclarationSyntax>().First().Identifier.Text;

			var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// remove the direct default value assignment
			var newDeclaration = declaration.WithInitializer(null)
				.WithSemicolonToken(default)
				.WithAdditionalAnnotations(Formatter.Annotation);
			root = root.ReplaceNode(declaration, newDeclaration);

			// Syntax tree has changed, locate the containing class
			var containingClass = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.First(c => c.Identifier.Text.Equals(containingClassName));

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
					.First(c => c.Identifier.Text.Equals(containingClassName));

				defaultValuesMethod = containingClass.Members.OfType<MethodDeclarationSyntax>()
					.FirstOrDefault(method => method.Identifier.Text.Equals("SetDefaultValues"));
			}

			// Look after an existing assignment to the property inside SetDefaultValues method
			ExpressionStatementSyntax propDefaultValueStatement = null;
			foreach (var statement in defaultValuesMethod.Body.DescendantNodes()
				.OfType<ExpressionStatementSyntax>()
				.Where(ess => ess.Expression is AssignmentExpressionSyntax))
			{
				var aes = (AssignmentExpressionSyntax)statement.Expression;
				// We are looking at an assignment, make sure it is assigning to the property we are applying a fix for
				if (aes.Left is IdentifierNameSyntax ins && ins.Identifier.Text.Equals(declaration.Identifier.Text))
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
						SyntaxFactory.IdentifierName(declaration.Identifier.Text),
						declaration.Initializer.Value));

				var newDefValuesMethod = defaultValuesMethod.AddBodyStatements(assignmentStatement);
				root = root.ReplaceNode(defaultValuesMethod, newDefValuesMethod);
			}
			else
			{
				// Update existing assignment
				var assignmentExpression = propDefaultValueStatement.Expression as AssignmentExpressionSyntax;
				var newAssignmentExpression = assignmentExpression.WithRight(declaration.Initializer.Value);
				root = root.ReplaceNode(assignmentExpression, newAssignmentExpression);
			}

			// Update the document to use the new root
			return document.WithSyntaxRoot(root);
		}

		private MethodDeclarationSyntax CreateDefaultValuesMethod()
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

		private ExpressionStatementSyntax CreateDefaultValuesMethod_CallBaseMethod()
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
