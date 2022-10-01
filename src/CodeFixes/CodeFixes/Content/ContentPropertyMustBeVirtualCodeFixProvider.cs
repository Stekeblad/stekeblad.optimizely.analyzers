using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Analyzer = Stekeblad.Optimizely.Analyzers.Analyzers.Content.ContentPropertyMustBeVirtualAnalyzer;

namespace Stekeblad.Optimizely.Analyzers.CodeFixes.Content
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContentPropertyMustBeVirtualCodeFixProvider)), Shared]
    public class ContentPropertyMustBeVirtualCodeFixProvider : MyCodeFixProviderBase<Analyzer>
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

            foreach (var x in context.Document.Project.MetadataReferences)
            {
                Debug.WriteLine(x.Display);
            }

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent as PropertyDeclarationSyntax;

            // Register a code action that will invoke the fix.
            CodeAction action = CodeAction.Create(
                title: Analyzer.Title.ToString(),
                createChangedDocument: c => AddVirtualKeyword(context.Document, declaration, c),
                equivalenceKey: Analyzer.DiagnosticId);

            context.RegisterCodeFix(action, diagnostic);
        }

        private async Task<Document> AddVirtualKeyword(Document document, PropertyDeclarationSyntax propDecl, CancellationToken cancellationToken)
        {
            var newPropDecl = propDecl.AddModifiers(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(propDecl, newPropDecl);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
