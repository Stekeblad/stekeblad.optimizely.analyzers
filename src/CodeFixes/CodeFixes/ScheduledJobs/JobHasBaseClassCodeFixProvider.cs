using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs.JobHasBaseClassAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.ScheduledJobs
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JobHasBaseClassCodeFixProvider)), Shared]
	public class JobHasBaseClassCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.DiagnosticId); }
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
				createChangedDocument: c => AddBaseClassFix(context.Document, declaration, c),
				equivalenceKey: Analyzer.DiagnosticId);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> AddBaseClassFix(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
		{
			// Create syntax for ScheduledJobBase
			TypeSyntax baseClassTypeName = SyntaxFactory.ParseTypeName("ScheduledJobBase");
			SimpleBaseTypeSyntax simpleBaseType = SyntaxFactory.SimpleBaseType(baseClassTypeName);

			// create a list for the base type
			var baseTypeSyntaxList = new SeparatedSyntaxList<BaseTypeSyntax>();
			baseTypeSyntaxList = baseTypeSyntaxList.Add(simpleBaseType);
			BaseListSyntax baseTypeList = SyntaxFactory.BaseList(baseTypeSyntaxList);

			// Update declaration, root and document
			var newDecl = declaration.WithBaseList(baseTypeList);
			var root = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);
			root = root.ReplaceNode(declaration, newDecl);
			return document.WithSyntaxRoot(root);
		}
	}
}
