using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.PropertyDefinitionTypePlugInAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.Content
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyDefinitionTypePlugInCodeFixProvider)), Shared]
	public class PropertyDefinitionTypePlugInCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Analyzer.MissingAttributeDiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = (CompilationUnitSyntax)await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent as TypeDeclarationSyntax;

			// Register a code action that will invoke the fix.
			CodeAction action = CodeAction.Create(
				title: Analyzer.MissingAttributeTitle,
				createChangedDocument: _ => AddPropertyDefinitionTypePlugInAttribute(context.Document, declaration, root),
				equivalenceKey: Analyzer.MissingAttributeDiagnosticId);

			context.RegisterCodeFix(action, diagnostic);
		}

		private Task<Document> AddPropertyDefinitionTypePlugInAttribute(Document document,
			TypeDeclarationSyntax typeDecl,
			CompilationUnitSyntax syntaxRoot)
		{
			CodeFixHelpers.AddAttributeDeclaration(ref document, ref syntaxRoot, typeDecl,
				"PropertyDefinitionTypePlugIn");

			CodeFixHelpers.AddUsingIfMissing(ref document, ref syntaxRoot, "EPiServer.PlugIn");

			return Task.FromResult(document);
		}
	}
}
