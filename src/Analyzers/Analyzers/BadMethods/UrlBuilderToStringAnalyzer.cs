using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.BadMethods
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UrlBuilderToStringAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string DiagnosticID = "SOA1018";
        public const string Title = "Cast UrlBuilder to string instead of calling ToString method";
        internal const string MessageFormat = "Cast UrlBuilder to string instead of calling ToString method to get a properly escaped url";

        internal static DiagnosticDescriptor UrlBuilderToStringRule =
            new DiagnosticDescriptor(DiagnosticID, Title, MessageFormat, Constants.Categories.BadMethods,
                DiagnosticSeverity.Info, true, helpLinkUri: HelpUrl(DiagnosticID));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UrlBuilderToStringRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol urlBuilderSymbol = startContext.Compilation.GetTypeByMetadataName(
                        "EPiServer.UrlBuilder");

                if (urlBuilderSymbol != null)
                {
                    startContext.RegisterOperationAction(
                        nodeContext => AnalyzeMethodCall(nodeContext, urlBuilderSymbol),
                        OperationKind.Invocation);
                }
            });
        }

        private static void AnalyzeMethodCall(OperationAnalysisContext context, INamedTypeSymbol urlBuilderSymbol)
        {
            var operation = (IInvocationOperation)context.Operation;

            // The remarks section on EPiServer.UrlBuilder.ToString() says the ToString() method
            // should only be used to create human-friendly debug output. To create a properly
            // escaped url you should cast the builder to a string like (string)urlBuilder
            if (operation.TargetMethod.Name.Equals("ToString")
                && SymbolEqualityComparer.Default.Equals(operation.TargetMethod.ContainingType, urlBuilderSymbol))
            {
                var diagnostic = Diagnostic.Create(UrlBuilderToStringRule, operation.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
