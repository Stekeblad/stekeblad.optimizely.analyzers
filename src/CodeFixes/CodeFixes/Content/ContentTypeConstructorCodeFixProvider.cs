using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.SyntaxFactories;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentTypeConstructorAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.Content
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContentTypeConstructorCodeFixProvider)), Shared]
	public class ContentTypeConstructorCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.ConstructorInitializationDiagnosticID); }
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

			// Find the property assignment inside the constructor identified by the diagnostic.
			var assignment = root.FindToken(diagnosticSpan.Start)
				.Parent
				.Ancestors()
				.OfType<ExpressionStatementSyntax>()
				.First();

			// Register a code action that will invoke the fix.
			CodeAction action = CodeAction.Create(
				title: Analyzer.ConstructorInitializationTitle,
				createChangedDocument: cancellationToken => PerformFix(context.Document, assignment, cancellationToken),
				equivalenceKey: Analyzer.ConstructorInitializationDiagnosticID);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> PerformFix(Document document, ExpressionStatementSyntax assignment, CancellationToken cancellationToken)
		{
			//Take note of the class the property is declared in.
			var containingClassName = assignment.Ancestors()
				.OfType<ClassDeclarationSyntax>()
				.First().Identifier.Text;

			var root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// remove initialization of the property from the constructor
			var ctor = assignment.Parent;
			var newCtor = ctor.RemoveNode(assignment, SyntaxRemoveOptions.KeepNoTrivia);
			root = root.ReplaceNode(ctor, newCtor);

			// Syntax tree has changed, locate the containing class
			var containingClass = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.First(c => c.Identifier.Text.Equals(containingClassName));

			// Ensure the method SetDefaultValues exists
			MethodDeclarationSyntax defaultValuesMethod = SetDefaultValuesSyntaxFactory.FindOrCreate(
				ref document, ref root, containingClass);

			// Add (or modify) assignment to the diagnosed property inside SetDefaultValues,
			// assign the same value it was assigned in the constructor
			var assignmentExpr = assignment.Expression as AssignmentExpressionSyntax;

			IdentifierNameSyntax left = assignmentExpr.Left as IdentifierNameSyntax
				?? throw new System.Exception($"[{nameof(ContentTypeConstructorCodeFixProvider)}] Could not cast {nameof(assignmentExpr)}.Left to IdentifierNameSyntax, actual type={assignmentExpr.Left.GetType().Name}");
			ExpressionSyntax right = assignmentExpr.Right;

			SetDefaultValuesSyntaxFactory.AddOrUpdateDefaultValue(
				ref document, ref root, ref defaultValuesMethod, left.Identifier.Text, right);

			return document;
		}
	}
}
