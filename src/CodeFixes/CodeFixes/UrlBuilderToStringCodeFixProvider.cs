using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.UrlBuilderToStringAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Analyzer)), Shared]
	public class UrlBuilderToStringCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Analyzer.DiagnosticID);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = (CompilationUnitSyntax)await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			// Find the location of the diagnostic
			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the ToString invocation expression and the UrlBuilder variable token inside it
			var toStringInvocationSyntax = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;
			SyntaxToken builderVariableToken = root.FindToken(diagnosticSpan.Start);

			CodeAction action = CodeAction.Create(
				title: Analyzer.Title,
				createChangedDocument: c => ReplaceWithCast(context.Document, toStringInvocationSyntax, builderVariableToken, c),
				equivalenceKey: Analyzer.DiagnosticID);

			context.RegisterCodeFix(action, diagnostic);
		}

		private async Task<Document> ReplaceWithCast(Document document,
			InvocationExpressionSyntax toStringInvocationSyntax,
			SyntaxToken builderVariableToken,
			CancellationToken c)
		{
			var syntaxRoot = (CompilationUnitSyntax)await document
				.GetSyntaxRootAsync(c)
				.ConfigureAwait(false);

			// Create syntax for casting builderVariableToken to string
			var castSyntax = SyntaxFactory.CastExpression(
				SyntaxFactory.ParseTypeName("string"),
				SyntaxFactory.ParseExpression(builderVariableToken.Text));

			// Replace ToString call in source with a cast: builder.ToString() --> (string)builder
			syntaxRoot = syntaxRoot.ReplaceNode(toStringInvocationSyntax, castSyntax);

			return document.WithSyntaxRoot(syntaxRoot);
		}
	}
}
