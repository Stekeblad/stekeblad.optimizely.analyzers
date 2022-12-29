using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stekeblad.Optimizely.Analyzers.Extensions;
using System.Collections.Immutable;

namespace Stekeblad.Optimizely.Analyzers.Analyzers.InitializableModules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InitializableModuleMissingInterfaceAnalyzer : MyDiagnosticAnalyzerBase
    {
        public const string DiagnosticId = "SOA1005";
        public const string Title = "Implement the interface IInitializableModule or IConfigurableModule";
        internal const string MessageFormat = "'{0}' is decorated with {1} but does not implement the interface IInitializableModule or IConfigurableModule";
        internal const string Category = Constants.Categories.InitializationModules;

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
                INamedTypeSymbol initModuleAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
                     "EPiServer.Framework.InitializableModuleAttribute");
                INamedTypeSymbol moduleDependsAttributeSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Framework.ModuleDependencyAttribute");
                INamedTypeSymbol initModuleInterfaceSymbol = startContext.Compilation.GetTypeByMetadataName(
                    "EPiServer.Framework.IInitializableModule");

                if (initModuleAttributeSymbol != null)
                {
                    startContext.RegisterSymbolAction(
                        nodeContext => TestForInterface(nodeContext, initModuleAttributeSymbol, moduleDependsAttributeSymbol, initModuleInterfaceSymbol),
                        SymbolKind.NamedType);
                }
            });
        }

        private static void TestForInterface(SymbolAnalysisContext context,
            INamedTypeSymbol initModuleAttributeSymbol,
            INamedTypeSymbol moduleDependsAttributeSymbol,
            INamedTypeSymbol initModuleInterfaceSymbol)
        {
            var analyzedSymbol = (INamedTypeSymbol)context.Symbol;

            //Check if auto-generated, abstract or not a class.
            if (analyzedSymbol.IsImplicitlyDeclared
                || analyzedSymbol.TypeKind != TypeKind.Class
                || analyzedSymbol.IsAbstract)
            {
                return;
            }

            // If neither InitializationModuleAttribute or ModuleDependencyAttribute is present then abort.
            if (!analyzedSymbol.TryGetAttributeDerivedFrom(initModuleAttributeSymbol, out AttributeData attributeData)
                && !analyzedSymbol.TryGetAttributeDerivedFrom(moduleDependsAttributeSymbol, out attributeData))
            {
                return;
            }

            // If symbol class implements IInitializableModule then we have nothing to report.
            // No need to test for IConfigurableModule as it implements IInitializableModule
            if (analyzedSymbol.AllInterfaces.Contains(initModuleInterfaceSymbol))
                return;

            // Type is decorated with InitializationModuleAttribute or ModuleDependencyAttribute
            // but does not implement IInitializableModule or IConfigurableModule, report it
            var diagnostic = Diagnostic.Create(Rule, analyzedSymbol.Locations[0], analyzedSymbol.Name, attributeData.AttributeClass.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
