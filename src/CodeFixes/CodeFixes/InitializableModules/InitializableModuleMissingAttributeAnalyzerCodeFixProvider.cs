using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Stekeblad.Optimizely.Analyzers.Helpers;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules.InitializableModuleMissingAttributeAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.InitializableModules
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitializableModuleMissingAttributeAnalyzer)), Shared]
    public class InitializableModuleMissingAttributeAnalyzer : MyCodeFixProviderBase<Analyzer>
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
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent as TypeDeclarationSyntax;

            // Register a code action that will invoke the fix.
            CodeAction actionA = CodeAction.Create(
                title: "Decorate with InitializableModuleAttribute",
                createChangedDocument: c => AddInitializableModuleAttribute(context.Document, declaration, c),
                equivalenceKey: Analyzer.DiagnosticId + "a");

            CodeAction actionB = CodeAction.Create(
                title: "Decorate with ModuleDependencyAttribute",
                createChangedDocument: c => AddModuleDependencyAttribute(context.Document, declaration, c),
                equivalenceKey: Analyzer.DiagnosticId + "b");

            context.RegisterCodeFix(actionA, diagnostic);
            context.RegisterCodeFix(actionB, diagnostic);
        }

        private async Task<Document> AddInitializableModuleAttribute(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var syntaxRoot = (CompilationUnitSyntax)await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            CodeFixHelpers.AddAttributeDeclaration(ref document, ref syntaxRoot, typeDecl,
                "InitializableModule");

            CodeFixHelpers.AddUsingIfMissing(ref document, ref syntaxRoot, "EPiServer.Framework");

            return document;
        }

        private async Task<Document> AddModuleDependencyAttribute(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var syntaxRoot = (CompilationUnitSyntax)await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            CodeFixHelpers.AddAttributeDeclaration(ref document, ref syntaxRoot, typeDecl,
                "ModuleDependency", "typeof(EPiServer.Web.InitializationModule)");

            CodeFixHelpers.AddUsingIfMissing(ref document, ref syntaxRoot, "EPiServer.Framework");

            return document;
        }
    }
}
