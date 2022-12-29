using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.ScheduledJobs
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JobHasBaseClassAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string DiagnosticId = "SOA1006";
        public const string Title = "Inherit from ScheduledJobBase";
        internal const string MessageFormat = "The scheduled job '{0}' does not inherit from ScheduledJobBase";
        internal const string Category = Constants.Categories.ScheduledJobs;

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning,
                true, helpLinkUri: HelpUrl(DiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol attribtuteSymbol = startContext.Compilation.GetTypeByMetadataName(
                "EPiServer.PlugIn.ScheduledPlugInAttribute");
                INamedTypeSymbol interfaceTypeSymbol = startContext.Compilation.GetTypeByMetadataName(
                      "EPiServer.Scheduler.Internal.IScheduledJob");

                if (attribtuteSymbol != null && interfaceTypeSymbol != null)
                {
                    startContext.RegisterSymbolAction(
                        nodeContext => AnalyzeJobDefinition(nodeContext, attribtuteSymbol, interfaceTypeSymbol),
                        SymbolKind.NamedType);
                }
            });
        }

        private static void AnalyzeJobDefinition(SymbolAnalysisContext context, INamedTypeSymbol attribtuteSymbol, INamedTypeSymbol interfaceTypeSymbol)
        {
            var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

            //Check if auto-generated, abstract or not a class.
            if (analyzedSymbol.IsImplicitlyDeclared
                || analyzedSymbol.TypeKind != TypeKind.Class
                || analyzedSymbol.IsAbstract)
            {
                return;
            }

            // Test for ScheduledPlugInAttribute and if not present then abort.
            if (!analyzedSymbol.TryGetAttributeDerivedFrom(attribtuteSymbol, out _))
            {
                return;
            }

            // If symbol class implements IScheduledJob then we have nothing to report.
            if (analyzedSymbol.AllInterfaces.Contains(interfaceTypeSymbol))
                return;

            // Symbol is decorated with ScheduledPlugInAttribute but does not implement/inherit required interface/base class. Report it
            var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
