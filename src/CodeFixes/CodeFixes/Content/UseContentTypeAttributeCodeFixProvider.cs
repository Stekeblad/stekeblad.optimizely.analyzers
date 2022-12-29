using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.UseContentTypeAttributeAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseContentTypeAttributeCodeFixProvider)), Shared]
	public class UseContentTypeAttributeCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.MissingAttributeDiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
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
				title: Analyzer.Title,
				createChangedDocument: _ => AddContentTypeAttribute(context.Document, declaration, root),
				equivalenceKey: Analyzer.MissingAttributeDiagnosticId);

			context.RegisterCodeFix(action, diagnostic);
		}

		private Task<Document> AddContentTypeAttribute(Document document,
			TypeDeclarationSyntax typeDecl,
			CompilationUnitSyntax syntaxRoot)
		{
			CodeFixHelpers.AddAttributeDeclaration(ref document, ref syntaxRoot, typeDecl,
				"ContentType", @"GroupName = ""Content""");

			CodeFixHelpers.AddUsingIfMissing(ref document, ref syntaxRoot, "EPiServer.DataAnnotations");

			return Task.FromResult(document);
		}
	}
}
