using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Stekeblad.Optimizely.Analyzers.SyntaxFactories;
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

			MethodDeclarationSyntax defaultValuesMethod = SetDefaultValuesSyntaxFactory.FindOrCreate(
				ref document, ref root, containingClass);

			SetDefaultValuesSyntaxFactory.AddOrUpdateDefaultValue(
				ref document, ref root, ref defaultValuesMethod, declaration.Identifier.Text, declaration.Initializer.Value);

			return document;
		}
	}
}
