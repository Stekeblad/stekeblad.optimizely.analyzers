using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.UseScheduledPluginAttributeAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseScheduledPluginAttributeAnalyzer)), Shared]
	public class JobHasNoAttributeCodeFixProvider : MyCodeFixProviderBase<Analyzer>
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
				title: Analyzer.MissingAttributeTitle,
				createChangedDocument: c => AddAttributeFix(context.Document, declaration, c),
				equivalenceKey: Analyzer.MissingAttributeDiagnosticId);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> AddAttributeFix(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
		{
			var syntaxRoot = (CompilationUnitSyntax)await document
				.GetSyntaxRootAsync(c)
				.ConfigureAwait(false);

			CodeFixHelpers.AddAttributeDeclaration(ref document, ref syntaxRoot, declaration,
				"ScheduledPlugIn");

			CodeFixHelpers.AddUsingIfMissing(ref document, ref syntaxRoot, "EPiServer.PlugIn");

			return document;
		}
	}
}
