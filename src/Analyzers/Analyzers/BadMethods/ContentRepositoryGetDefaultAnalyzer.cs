using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContentRepositoryGetDefaultAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string UseGetDefaultDiagnosticId = "SOA1026";
        public const string UseGetDefaultTitle = "Use IContentRepository.GetDefault<T>()";
        internal const string UseGetDefaultMessageFormat = "Do not create content instances with the 'new' keyword, use IContentRepository.GetDefault<{0}>(ContentReference parentLink) instead";
        internal const string UseGetDefaultCategory = Constants.Categories.BadMethods;

        internal static DiagnosticDescriptor UseGetDefaultRule =
            new DiagnosticDescriptor(UseGetDefaultDiagnosticId, UseGetDefaultTitle, UseGetDefaultMessageFormat, UseGetDefaultCategory, DiagnosticSeverity.Warning,
                true, helpLinkUri: HelpUrl(UseGetDefaultDiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UseGetDefaultRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol iContentRepositorySymbol = startContext.Compilation.GetTypeByMetadataName(
                     "EPiServer.IContentRepository");

                INamedTypeSymbol iContentDataSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Core.IContentData");

                if (iContentRepositorySymbol != null && iContentDataSymbol != null)
                {
                    startContext.RegisterOperationAction(
                        nodeContext => TestForIncorrectlyCreatedContent(nodeContext, iContentDataSymbol), OperationKind.ObjectCreation);
                }
            });
        }

        private static void TestForIncorrectlyCreatedContent(OperationAnalysisContext context, INamedTypeSymbol iContentDataSymbol)
        {
            var operation = (IObjectCreationOperation)context.Operation;

            var typeBeingCreated = operation.Type as INamedTypeSymbol;
            if (typeBeingCreated.AllInterfaces.Contains(iContentDataSymbol))
            {
                var diagnostic = Diagnostic.Create(UseGetDefaultRule, operation.Syntax.GetLocation(), typeBeingCreated.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
