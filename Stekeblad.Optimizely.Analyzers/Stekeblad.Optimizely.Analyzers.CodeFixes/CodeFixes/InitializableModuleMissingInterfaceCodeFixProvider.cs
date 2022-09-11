using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModuleMissingInterfaceAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitializableModuleMissingInterfaceCodeFixProvider)), Shared]
	public class InitializableModuleMissingInterfaceCodeFixProvider : MyCodeFixProviderBase<Analyzer>
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(Analyzer.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public async override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics[0];
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var declaration = root.FindToken(diagnosticSpan.Start).Parent as TypeDeclarationSyntax;

			CodeAction actionA = CodeAction.Create(
				title: "Implement IInitializableModule interface",
				createChangedDocument: c => AddIInitializableModuleInterface(context.Document, declaration, c),
				equivalenceKey: Analyzer.DiagnosticId + "a");

			CodeAction actionB = CodeAction.Create(
				title: "Implement IConfigurableModule interface",
				createChangedDocument: c => AddIConfigurableModuleInterface(context.Document, declaration, c),
				equivalenceKey: Analyzer.DiagnosticId + "b");

			context.RegisterCodeFix(actionA, diagnostic);
			context.RegisterCodeFix(actionB, diagnostic);
		}

		private async Task<Document> AddIInitializableModuleInterface(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
		{
			document = await AddSimpleBaseTypeToTypeDeclaration(
				document, typeDecl, cancellationToken, "IInitializableModule");

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var compilationUnit = root as CompilationUnitSyntax;
			CodeFixHelpers.AddUsingIfMissing(ref document, ref compilationUnit, "EPiServer.Framework");

			return document;
		}

		private async Task<Document> AddIConfigurableModuleInterface(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
		{
			document = await AddSimpleBaseTypeToTypeDeclaration(
				document, typeDecl, cancellationToken, "IConfigurableModule");

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var compilationUnit = root as CompilationUnitSyntax;
			CodeFixHelpers.AddUsingIfMissing(ref document, ref compilationUnit, "EPiServer.ServiceLocation");

			return document;
		}

		private async Task<Document> AddSimpleBaseTypeToTypeDeclaration(Document document,
			TypeDeclarationSyntax typeDecl,
			CancellationToken cancellationToken,
			string simpleTypeName)
		{
			// Create syntax for the interface
			var interfaceTypeName = SyntaxFactory.ParseTypeName(simpleTypeName);
			var simpleBaseType = SyntaxFactory.SimpleBaseType(interfaceTypeName);

			// Add the syntax to the type declaration and update the root node
			// TODO: Code fix result is not formatted as I want it. Interface is placed on new row, can it be placed on same row as type declaration? Is it based on a VS setting I can't find?
			var newTypeDecl = typeDecl.AddBaseListTypes(simpleBaseType);
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = root.ReplaceNode(typeDecl, newTypeDecl);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
